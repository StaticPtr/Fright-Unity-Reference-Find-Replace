//
// MIT License
// 
// Copyright (c) 2019 Brandon Dahn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Version = System.Version;

namespace Fright.Editor.Templates
{
	/// Describes a template made from Xml that can be used to make a C# file
	public class XmlTemplate : XmlBase
	{
		public static readonly Version MINIMUM_SUPPORTED_TEMPLATE_FORMAT = new Version("1.0.0.0");
		public static readonly Version MAXIMUM_SUPPORTED_TEMPLATE_FORMAT = new Version("1.0.0.0");
		private static Dictionary<string, System.Type> _xmlBaseTypes;

		public bool isMalformed;
		public int priority;
		public System.Version templateFormat;
		public List<XmlUsingNamespace> usings = new List<XmlUsingNamespace>();
		public List<XmlBuildOption> buildOptions = new List<XmlBuildOption>();
		public List<XmlBase> children = new List<XmlBase>();

		private static Dictionary<string, System.Type> xmlBaseTypes
		{
			get
			{
				if (_xmlBaseTypes == null)
				{
					FindAllTagTypes();
				}
				return _xmlBaseTypes;
			}
		}

		public override string xmlType
		{
			get { return "template"; }
		}

		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);

			//One offs
			templateFormat = new System.Version(node.GetAttribute("format", "1.0"));
			priority = node.GetAttribute<int>("priority", 0);

			if (IsTemplateFormatSupported(templateFormat))
			{
				bool hasAddedUsingPlaceholder = false;

				//Children
				foreach(XmlNode child in node.ChildNodes)
				{
					XmlBase xmlBase = CreateXmlObjectFromNode(child, document);

					if (xmlBase != null)
					{
						if (xmlBase is XmlUsingNamespace)
						{
							usings.Add(xmlBase as XmlUsingNamespace);

							if (!hasAddedUsingPlaceholder)
							{
								children.Add(new XmlUsingNamespacePlaceholder());
								hasAddedUsingPlaceholder = true;
							}
						}
						else if(xmlBase is XmlBuildOption)
						{
							buildOptions.Add(xmlBase as XmlBuildOption);
						}
						else
						{
							children.Add(xmlBase);
						}
					}
				}
			}
			else
			{
				isMalformed = true;
			}
		}

		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			ChildrenToCSharp(stringBuilder, indentationLevel, settings, GetSerializableChildren(settings));
		}

		/// Converts multiple XmlBase objects into C#
		public static void ChildrenToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings, IEnumerable<XmlBase> children)
		{
			bool isFirstChild = true;

			foreach (XmlBase child in children)
			{
				if (child.ShouldUse(settings))
				{
					if (isFirstChild)
					{
						isFirstChild = false;
					}
					else
					{
						stringBuilder.Append("\n");
					}

					child.ToCSharp(stringBuilder, indentationLevel, settings);
				}
			}
		}

		public string GetMetaData(string key, string fallback = null)
		{
			string result = fallback;

			//Find the first XmlMetaData that matches the provided key
			for(int i = 0; i < children.Count; ++i)
			{
				if (children[i] is XmlMetaData metaData)
				{
					if (metaData.key.Equals(key, System.StringComparison.InvariantCultureIgnoreCase))
					{
						result = metaData.value;
					}
				}
			}

			//Return the result
			return result;
		}

		public IEnumerable<XmlBase> GetSerializableChildren(TemplateSettings settings)
		{
			//Other children
			foreach(var child in children)
			{
				//The usings are not included in the children list, therefor a placeholder is used to determine where to inject them
				if (child is XmlUsingNamespacePlaceholder)
				{
					foreach(var @using in GetUsingNamespaceChildren(settings))
					{
						yield return @using;
					}

					continue;
				}

				//Regular child
				if (child.ShouldUse(settings))
				{
					yield return child;
				}
			}
		}

		private IEnumerable<XmlBase> GetUsingNamespaceChildren(TemplateSettings settings)
		{
			//Template usings
			foreach(var @using in usings)
			{
				if (!@using.isOptional || settings == null || settings.IsOptionalUsingEnabled(@using.id))
				{
					yield return @using;
				}
			}

			//Custom usings
			if (settings != null && settings.optionalUsings != null)
			{
				foreach(var @using in settings.optionalUsings)
				{
					if (@using.isCustom && @using.isEnabled && !string.IsNullOrEmpty(@using.id))
					{
						yield return new XmlUsingNamespace()
						{
							id = @using.id,
						};
					}
				}
			}
		}

		/// Constructs an XmlBase from the node if it's type is known
		public static XmlBase CreateXmlObjectFromNode(XmlNode node, XmlDocument document)
		{
			XmlBase result = null;
			System.Type nodeType = null;

			//Instantiate the type if it's a known type
			if (xmlBaseTypes.TryGetValue(node.LocalName.ToLower(), out nodeType) && nodeType != null)
			{
				if (typeof(XmlBase).IsAssignableFrom(nodeType) && !nodeType.IsAbstract)
				{
					result = System.Activator.CreateInstance(nodeType) as XmlBase;
					result.ConstructFromXml(node, document);
				}
			}
			else if (node is XmlText)
			{
				result = new XmlCodeblock();
				result.ConstructFromXml(node, document);
			}

			//Return the result
			return result;
		}

		private static void FindAllTagTypes()
		{
			_xmlBaseTypes = new Dictionary<string, System.Type>();

			//Find all the XmlBase types in any of the assemblies
			foreach(Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach(System.Type type in assembly.GetExportedTypes())
					{
						if (!type.IsAbstract && !type.IsGenericType && typeof(XmlBase).IsAssignableFrom(type))
						{
							try
							{
								XmlBase obj = System.Activator.CreateInstance(type) as XmlBase;
								xmlBaseTypes[obj.xmlType.ToLower()] = type;
							}
							catch (System.Exception e)
							{
								UnityEngine.Debug.LogException(e);
							}
						}
					}
				}
				catch {}
			}
		}

		#region Statics
		public static XmlTemplate FromFile(string path)
		{
			XmlTemplate result = null;

			if (File.Exists(path))
			{
				XmlDocument document = new XmlDocument();
				document.Load(path);

				XmlNode templateNode = document.GetFirstChild("template");
				result = new XmlTemplate();
				result.ConstructFromXml(templateNode, document);
			}

			return result;
		}

		public static bool IsTemplateFormatSupported(Version version)
		{
			return version >= MINIMUM_SUPPORTED_TEMPLATE_FORMAT && version <= MAXIMUM_SUPPORTED_TEMPLATE_FORMAT;
		}
		#endregion
	}
}