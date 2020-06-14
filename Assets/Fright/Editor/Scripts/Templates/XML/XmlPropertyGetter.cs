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
	public class XmlPropertyGetter : XmlCSharpBase
	{
		public virtual string keyword { get { return "get"; } }

		public override string xmlType
		{
			get { return "getter"; }
		}

		/// Should this XmlBase be used (and converted to C#)
		public override bool ShouldUse(TemplateSettings settings)
		{
			return true;
		}

		/// Constructs the object from an Xml node and document
		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			accessibility = node.GetAttribute("access", string.Empty);
		}

		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings templateSettings)
		{
			if (children.Count > 0)
			{
				//Accessibility
				stringBuilder.AppendIndentations(indentationLevel);
				TemplateBuilder.BeginColorBlock(stringBuilder, templateSettings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);
				stringBuilder.AppendIf(accessibility + " ", !string.IsNullOrEmpty(accessibility));
				TemplateBuilder.EndColorBlock(stringBuilder, templateSettings, TemplateSettings.ACCESSIBILITY_KEYWORD_COLOR);

				//Get or Set
				TemplateBuilder.BeginColorBlock(stringBuilder, templateSettings, TemplateSettings.SYSTEM_KEYWORD_COLOR);
				stringBuilder.Append(keyword);
				TemplateBuilder.EndColorBlock(stringBuilder, templateSettings, TemplateSettings.SYSTEM_KEYWORD_COLOR);

				//Body
				BodyToCSharp(stringBuilder, indentationLevel, templateSettings);
			}
		}

		protected virtual void BodyToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings templateSettings)
		{
			if (children.Count == 1 && children[0] is XmlCodeblock && !(children[0] as XmlCodeblock).body.Contains("\n"))
			{
				stringBuilder.Append(" { ");
				XmlTemplate.ChildrenToCSharp(stringBuilder, 0, templateSettings, children);
				stringBuilder.Append(" }");
			}
			else
			{
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("{\n");
				XmlTemplate.ChildrenToCSharp(stringBuilder, indentationLevel + 1, templateSettings, children);
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("}");
			}
		}
	}
}