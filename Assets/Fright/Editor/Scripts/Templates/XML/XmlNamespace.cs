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
	public class XmlNamespace : XmlBaseWithChildren
	{
		public override string xmlType
		{
			get { return "namespace"; }
		}

		public override bool ShouldUse(TemplateSettings settings)
		{
			return true;
		}

		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			string modifiedID = GetModifiedID(settings);

			if (string.IsNullOrEmpty(modifiedID))
			{
				base.ToCSharp(stringBuilder, indentationLevel, settings);
			}
			else
			{
				//Start
				stringBuilder.AppendIndentations(indentationLevel);
				TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);
				stringBuilder.Append("namespace ");
				TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.SYSTEM_KEYWORD_COLOR);
				stringBuilder.Append(modifiedID);
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("{\n");

				//Body
				base.ToCSharp(stringBuilder, indentationLevel + 1, settings);

				//End
				stringBuilder.Append("\n");
				stringBuilder.AppendIndentations(indentationLevel);
				stringBuilder.Append("}");
			}
		}
	}
}