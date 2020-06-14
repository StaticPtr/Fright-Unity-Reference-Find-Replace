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
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Fright.Editor.Templates
{
	/// Describes a type that can be used in an XML Template
	public abstract class XmlType : XmlCSharpBase
	{
		public bool isSealed;
		public bool isPartial;
		public bool isStatic;
		public bool isAbstract;
		public string @base;
		public List<XmlInterfaceContract> interfaces = new List<XmlInterfaceContract>();

		public abstract string kind
		{
			get;
		}

		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			children.Clear();

			//One offs
			isSealed = node.GetAttribute("sealed", "false").Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
			isStatic = node.GetAttribute("static", "false").Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
			isPartial = node.GetAttribute("partial", "false").Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
			isAbstract = node.GetAttribute("abstract", "false").Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
			@base = node.GetAttribute("base");

			//Children
			foreach (XmlNode child in node.ChildNodes)
			{
				XmlBase xmlObj = XmlTemplate.CreateXmlObjectFromNode(child, document);

				if (xmlObj is XmlInterfaceContract)
				{
					interfaces.Add(xmlObj as XmlInterfaceContract);
				}
				else if (xmlObj != null)
				{
					children.Add(xmlObj);
				}
			}
		}

		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			//Signature
			stringBuilder.AppendIndentations(indentationLevel);
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);
			stringBuilder.Append(accessibility);
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);
			stringBuilder.AppendSpace();
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);
			stringBuilder.Append(kind);
			stringBuilder.AppendSpace();
			stringBuilder.AppendIf("static ", isStatic);
			stringBuilder.AppendIf("partial", isPartial);
			stringBuilder.AppendIf("abstract ", isAbstract);
			stringBuilder.AppendIf("sealed ", isSealed);
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);

			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
			stringBuilder.Append(id);
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);

			BuildBaseType(stringBuilder, settings);
			stringBuilder.Append("\n");

			//Body start
			stringBuilder.AppendIndentations(indentationLevel);
			stringBuilder.Append("{\n");

			//Body
			XmlTemplate.ChildrenToCSharp(stringBuilder, indentationLevel + 1, settings, children);

			//Body end
			stringBuilder.Append("\n");
			stringBuilder.AppendIndentations(indentationLevel);
			stringBuilder.Append("}");
		}

		private void BuildBaseType(StringBuilder stringBuilder, TemplateSettings settings)
		{
			//Add the base class
			if (!string.IsNullOrEmpty(@base))
			{
				stringBuilder.Append(" : ");
				TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
				stringBuilder.Append(@base);
				TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
			}

			//Add the interfaces
			for (int i = 0; i < interfaces.Count; ++i)
			{
				//Append the delimiter
				if (i == 0)
				{
					stringBuilder.Append(string.IsNullOrEmpty(@base) ? " : " : ", ");
				}
				else
				{
					stringBuilder.Append(", ");
				}

				//Add the interface
				TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
				stringBuilder.Append(interfaces[i].id);
				TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
			}
		}
	}
}