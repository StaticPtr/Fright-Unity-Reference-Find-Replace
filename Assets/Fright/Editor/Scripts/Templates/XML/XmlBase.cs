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

namespace Fright.Editor.Templates
{
	/// An abstract class for all Template XML objects
	public abstract class XmlBase
	{
		/// The identifier for the XML object
		public string id;

		/// An optional string for the text color of the generated C# of this tag.
		/// It is the responsibility of the TCSharp function to use this!
		public string textColor;

		/// The XML tag that this object comes from
		public abstract string xmlType { get; }

		/// Converts the XML object into C# and adds it to the string builder
		public abstract void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings);

		/// Should this XmlBase be used (and converted to C#)
		public virtual bool ShouldUse(TemplateSettings settings)
		{
			return !string.IsNullOrEmpty(GetModifiedID(settings));
		}

		/// Constructs the object from an Xml node and document
		public virtual void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			id = node.GetAttribute("id");
			textColor = node.GetAttribute("text-color");
		}

		/// Applies any text replacements to the ID of this object and returns the result
		public virtual string GetModifiedID(TemplateSettings settings)
		{
			string result = id;

			if (settings != null)
			{
				result = settings.ApplyReplacementsToText(result);
			}

			//Return the result
			return result;
		}
	}
}