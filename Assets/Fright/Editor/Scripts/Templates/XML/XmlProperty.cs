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
	public class XmlProperty : XmlCSharpBase
	{
		public string type;
		public string defaultValue;
		public bool isStatic;
		public Virtuality virtuality;

		/// The XML tag that this object comes from	
		public override string xmlType { get { return "property"; } }

		/// Constructs the object from an Xml node and document
		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			type = node.GetAttribute("type");
			isStatic = node.GetAttribute<bool>("static");
			defaultValue = node.GetAttribute("default");
			virtuality = node.GetEnumAttribute<Virtuality>("virtuality");
		}

		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			stringBuilder.AppendIndentations(indentationLevel);

			//Accessibility
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);
			stringBuilder.Append(accessibility);
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);

			//Static and Virtuality
			stringBuilder.AppendSpace();
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);
			stringBuilder.AppendIf("static ", isStatic);
			stringBuilder.AppendIf(virtuality.ToString() + " ", virtuality != Virtuality.none);
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);

			//Type
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);
			stringBuilder.Append(type ?? "?");
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.TYPE_COLOR);

			//Name
			stringBuilder.AppendSpace();
			stringBuilder.Append(id);

			//Children
			if (children.Count > 0)
			{
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("{\n");
				base.ToCSharp(stringBuilder, indentationLevel + 1, settings);
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("}");
			}
			else
			{
				//Default
				if (!string.IsNullOrEmpty(defaultValue))
				{
					stringBuilder.Append(" = ");

					if (type.Equals("string", System.StringComparison.InvariantCultureIgnoreCase))
					{
						stringBuilder.Append("\"");
						stringBuilder.Append(defaultValue);	
						stringBuilder.Append("\"");
					}
					else
					{
						stringBuilder.Append(defaultValue);
					}
				}

				stringBuilder.Append(';');
			}
		}
	}
}