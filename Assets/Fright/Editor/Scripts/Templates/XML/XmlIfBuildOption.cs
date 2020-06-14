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
	public class XmlIfBuildOption : XmlBase
	{
		public static char[] DELIMITERS = new [] {',', ';', ' '};

		public bool isAND = true;
		public string[] buildOptionsToCheck;
		public List<XmlBase> children = new List<XmlBase>();

		/// The XML tag that this object comes from	
		public override string xmlType { get { return "if-build-option"; } }
		
		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings templateSettings)
		{
			XmlTemplate.ChildrenToCSharp(stringBuilder, indentationLevel, templateSettings, children);
		}

		/// Constructs the object from an Xml node and document
		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			buildOptionsToCheck = node.GetAttribute("options", string.Empty).Split(DELIMITERS, System.StringSplitOptions.RemoveEmptyEntries);
			isAND = node.GetAttribute("operator", "and").Equals("and", System.StringComparison.InvariantCultureIgnoreCase);

			//Children
			foreach (XmlNode child in node.ChildNodes)
			{
				XmlBase xmlBase = XmlTemplate.CreateXmlObjectFromNode(child, document);

				if (xmlBase != null)
				{
					children.Add(xmlBase);
				}
			}
		}

		public override bool ShouldUse(TemplateSettings templateSettings)
		{
			return IsConditionMet(templateSettings);
		}

		public bool IsConditionMet(TemplateSettings templateSettings)
		{
			bool result = isAND;

			//Iterate over each option
			for(int i = 0; i < buildOptionsToCheck.Length; ++i)
			{
				string optionToCheck = buildOptionsToCheck[i];
				bool isFlipped = optionToCheck.Contains("!");
				bool isMet = "true".Equals(templateSettings.GetBuildOptionValue(optionToCheck.Replace("!", null)), System.StringComparison.InvariantCultureIgnoreCase);
			
				if (isFlipped)
				{
					isMet = !isMet;
				}

				if (isAND)
				{
					result &= isMet;
				}
				else
				{
					result |= isMet;
				}
			}

			//Return the result
			return result;
		}
	}
}