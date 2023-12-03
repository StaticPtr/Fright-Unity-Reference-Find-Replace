//
// MIT License
// 
// Copyright (c) 2020 Brandon Dahn
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
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Fright.Editor.References
{
	[Serializable]
	public class ReferenceQuery
	{
		private const string PROG_BAR_TITLE_FINDING = "Finding References";

		//Inputs
		public QueryWindowSettings Settings = new QueryWindowSettings();

		//Outputs
		public List<string> ReferencingPaths = new List<string>();
		
		/// Finds all the references to the object using the default object regex
		public void FindReferences(Object objectToFind) => FindReferences(AssetDatabase.IsMainAsset(objectToFind) ? RegexForAsset(objectToFind) : RegexForSubAsset(objectToFind));
		/// Finds all the references to the objects using the default object regex
		public void FindReferences(IList<Object?> objectsToFind) => FindReferences(RegexForAssets(objectsToFind));

		/// Finds all the references to the object using the provided regex
		public void FindReferences(string regex)
		{
			try
			{
				//Create a list of all the files to search
				EditorUtility.DisplayProgressBar(PROG_BAR_TITLE_FINDING, "Determining files to search", 0.0f);
				List<FileInfo> filesToSearch = FindFilesToSearch();

				//Create a regex and search the files for matches
				Regex searchRegex = new Regex(regex, RegexOptions.Compiled);
				ReferencingPaths = new List<string>(RegexFiles(filesToSearch, searchRegex));
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		/// Replaces any already found references using the provided regex and replacement
		public void ReplaceReferences(Object objectToFind, Object objectToReplace) => ReplaceReferences(RegexForAsset(objectToFind), RegexForAsset(objectToReplace));

		/// Replaces any already found references using the provided regex and replacement
		public void ReplaceReferences(string regex, string replacement)
		{
			try
			{
				Regex searchRegex = new Regex(regex, RegexOptions.Compiled);
				
				for(int i = 0; i < ReferencingPaths.Count; ++i)
				{
					string path = ReferencingPaths[i];

					if (EditorUtility.DisplayCancelableProgressBar(PROG_BAR_TITLE_FINDING, path, (float)i / (float)ReferencingPaths.Count))
					{
						Debug.LogError("[ReferenceQuery] Request cancelled by user");
						break;
					}

					string text = File.ReadAllText(path);
					text = searchRegex.Replace(text, replacement);
					File.WriteAllText(path, text);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();
				ReferencingPaths = new List<string>();
			}
		}

		private List<FileInfo> FindFilesToSearch()
		{
			DirectoryInfo folderToSearch = new DirectoryInfo(Settings.RootFolderToSearch);
			List<FileInfo> filesToSearch = new List<FileInfo>();

			foreach(var extension in Settings.ExtensionsToSearch)
			{
				filesToSearch.AddRange(folderToSearch.GetFiles("*" + extension, SearchOption.AllDirectories));
			}

			return filesToSearch;
		}

		private IEnumerable<string> RegexFiles(List<FileInfo> filesToSearch, Regex regex)
		{
			for (int i = 0; i < filesToSearch.Count; ++i)
			{
				var file = filesToSearch[i];
				string path = file.FullName;

				if (EditorUtility.DisplayCancelableProgressBar(PROG_BAR_TITLE_FINDING, path, (float)i / (float)filesToSearch.Count))
				{
					Debug.LogError("[ReferenceQuery] Request cancelled by user");
					break;
				}

				string text = File.ReadAllText(path);
				bool isMatch = regex.IsMatch(text);
				
				if (isMatch)
				{
					yield return path;
				}
			}
		}

		#region Static Functions
		public static string GetObjectGUID(Object obj) => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
		public static string RegexForAsset(Object obj) => $"guid: {GetObjectGUID(obj)}";
		public static string RegexForSubAsset(Object obj)
		{
			if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localID))
			{
				return $"fileID: {localID}, guid: {guid}";
			}
			return RegexForAsset(obj);
		}

		public static string RegexForAssets(IList<Object?> objects)
		{
			string allAssetsRegex = string.Empty;

			//Aggregate all of the regexes for each asset to find
			for(int i = 0; i < objects.Count; ++i)
			{
				Object? obj = objects[i];

				if (!obj)
					continue;

				string singleAssetRegex = AssetDatabase.IsMainAsset(obj!) ? RegexForAsset(obj!) : RegexForSubAsset(obj!);

				if (allAssetsRegex.Length == 0)
				{
					allAssetsRegex = $"({singleAssetRegex})";
				}
				else
				{
					allAssetsRegex += $"|({singleAssetRegex})";
				}
			}

			//Return the result
			return allAssetsRegex;
		}
		#endregion
	}
}