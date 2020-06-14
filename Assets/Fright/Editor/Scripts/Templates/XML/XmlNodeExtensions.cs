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
	public static class XmlNodeExtensions
	{
		public static string GetAttribute(this XmlNode node, string attributeName, string fallback = null)
		{
			string result = fallback;

			//Check if this node even has attribute
			if (node.Attributes != null)
			{
				var attribute = node.Attributes.GetNamedItem(attributeName);
				result = attribute != null ? attribute.Value : fallback;
			}

			//Return the result
			return result;
		}

		public static T GetAttribute<T>(this XmlNode node, string attributeName, T fallback = default(T))
		{
			T result = fallback;
			string stringValue = node.GetAttribute(attributeName);

			//Check if there is any value at all
			if (stringValue != null)
			{
				try
				{
					result = (T)System.Convert.ChangeType(stringValue, typeof(T));
				}
				catch
				{
					result = fallback;
				}
			}

			//Return the result
			return result;
		}

		public static TEnum GetEnumAttribute<TEnum>(this XmlNode node, string attributeName, TEnum fallback = default(TEnum))
		{
			TEnum result = fallback;
			string stringValue = node.GetAttribute(attributeName);

			//Check if there is any value at all
			if (stringValue != null)
			{
				try
				{
					result = (TEnum)System.Enum.Parse(typeof(TEnum), stringValue, true);
				}
				catch
				{
					result = fallback;
				}
			}

			//Return the result
			return result;
		}

		public static XmlNode GetFirstChild(this XmlDocument document, string nodeName)
		{
			XmlNode result = null;

			foreach(XmlNode node in document.ChildNodes)
			{
				if (node.LocalName.Equals(nodeName, System.StringComparison.InvariantCultureIgnoreCase))
				{
					result = node;
					break;
				}
			}

			return result;
		}
	}
}