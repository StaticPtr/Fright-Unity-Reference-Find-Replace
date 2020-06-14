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
using UnityEngine;

namespace Fright.Editor.Templates
{
	public static class TemplateBuilder
	{
		/// 4 Kibibytes (4096 bytes)
		public const int _4KiB = 4096;
		public const string WINDOWS_LINE_ENDINGS = "\r\n";
		public const string UNIX_LINE_ENDINGS = "\n";

		public static readonly string TEMPLATE_BUILDER_VERSION = "1.2.0";

		public static string BuildCodeFromTemplate(XmlTemplate template, TemplateSettings settings)
		{
			//Create the code
			StringBuilder codeBuilder = new StringBuilder(_4KiB);
			template.ToCSharp(codeBuilder, 0, settings);

			//Transform the code
			string code = NormalizeLineEndings(codeBuilder.ToString(), settings.lineEndings);
			code = NormalizeTabs(codeBuilder.ToString(), settings.tabMode);
			code = settings.ApplyReplacementsToText(code);

			//Return the result
			return code;
		}

		public static IEnumerable<XmlTemplate> FindAllTemplatesInProject()
		{
			const string TEMPLATE_EXTENSION = ".xtemplate";
			var files = Directory.GetFiles(Application.dataPath + "/", "*" + TEMPLATE_EXTENSION, SearchOption.AllDirectories);

			foreach(string filePath in files)
			{
				if (Path.GetExtension(filePath) == TEMPLATE_EXTENSION)
				{
					yield return XmlTemplate.FromFile(filePath);
				}
			}
		}

		/// Changes all of the line endings in the provided text into the specified format
		public static string NormalizeLineEndings(string textToFix, LineEndings lineEndings)
		{
			//Transform all the line endings into Unix so it's easier to manage in the next step
			textToFix =  textToFix.Replace(WINDOWS_LINE_ENDINGS, UNIX_LINE_ENDINGS);

			//Perform the final transformation
			switch(lineEndings)
			{
				case LineEndings.windows:
					textToFix = textToFix.Replace(UNIX_LINE_ENDINGS, WINDOWS_LINE_ENDINGS);
					break;

				case LineEndings.unix:
					//Do nothing
					break;
			}

			//Return the result
			return textToFix;
		}

		/// Changes all of the tabs in the provided text into the specified format
		public static string NormalizeTabs(string textToFix, TabMode tabMode)
		{
			//Perform the final transformation
			switch(tabMode)
			{
				case TabMode.tabs:
					textToFix = textToFix.Replace("    ", "\t");
					break;

				case TabMode.spaces:
					textToFix = textToFix.Replace("\t", "    ");
					break;
			}

			//Return the result
			return textToFix;
		}

		/// Starts a unity color tag to the string builder 
		public static void BeginColorBlock(StringBuilder stringBuilder, TemplateSettings templateSettings, string colorCode)
		{
			if (templateSettings != null && templateSettings.enableSyntaxHighlighting && !string.IsNullOrEmpty(colorCode))
			{
				stringBuilder.Append("<color=");
				stringBuilder.Append(colorCode);
				stringBuilder.Append(">");
			}
		}

		/// Closes a unity color tag to the string builder 
		public static void EndColorBlock(StringBuilder stringBuilder, TemplateSettings templateSettings, string colorCode)
		{
			if (templateSettings != null && templateSettings.enableSyntaxHighlighting && !string.IsNullOrEmpty(colorCode))
			{
				stringBuilder.Append("</color>");
			}
		}

		#region Embedded Types
		public enum LineEndings
		{
			/// Microsoft Windows \r\n
			windows,
			/// Unix and macOS \n
			unix,
		}

		public enum TabMode
		{
			/// Use regular tabs "\t"
			tabs,
			/// Use spaces "    "
			spaces,
		}
		#endregion
	}
}