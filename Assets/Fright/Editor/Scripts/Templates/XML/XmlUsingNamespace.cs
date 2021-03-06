﻿//
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
	public class XmlUsingNamespace : XmlBase
	{
		public bool isOnByDefault;
		public bool isOptional;

		public override string xmlType
		{
			get { return "using"; }
		}

		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			isOnByDefault = node.GetAttribute<bool>("default", true);
			isOptional = node.GetAttribute<bool>("optional");
		}

		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			TemplateBuilder.BeginColorBlock(stringBuilder, settings, TemplateSettings.USING_NAMESPACE_COLOR);
			{
				stringBuilder.Append("using ");
				stringBuilder.Append(id);
				stringBuilder.Append(";");
			}
			TemplateBuilder.EndColorBlock(stringBuilder, settings, TemplateSettings.USING_NAMESPACE_COLOR);
		}
	}

	internal class XmlUsingNamespacePlaceholder : XmlBase
	{
		public override string xmlType
		{
			get { return "using-placeholder"; }
		}

		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			throw new System.NotSupportedException();
		}

		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			throw new System.NotSupportedException();
		}
	}
}