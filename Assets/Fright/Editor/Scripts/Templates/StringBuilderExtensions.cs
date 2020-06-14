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
	internal static class StringBuilderExtensions
	{
		private static readonly string[] lineEndings = new[] { "\r\n", "\n" };

		public static void AppendSpace(this StringBuilder stringBuilder)
		{
			stringBuilder.Append(' ');
		}

		public static void AppendIf(this StringBuilder stringBuilder, string text, bool condition)
		{
			if (condition)
			{
				stringBuilder.Append(text);
			}
		}

		public static void AppendWithIndentation(this StringBuilder stringBuilder, string text, int indentation)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] lines = GetLines(text);

				for (int i = 0; i < lines.Length; ++i)
				{
					stringBuilder.AppendIndentations(indentation);
					stringBuilder.Append(lines[i]);

					if (i < lines.Length - 1)
					{
						stringBuilder.Append("\n");
					}
				}
			}
		}

		public static void AppendIndentations(this StringBuilder stringBuilder, int indentation)
		{
			//Append the indentation
			for (int j = 0; j < indentation; ++j)
			{
				stringBuilder.Append('\t');
			}
		}

		public static string[] GetLines(string text)
		{
			return text.Split(lineEndings, System.StringSplitOptions.None);
		}
	}
}