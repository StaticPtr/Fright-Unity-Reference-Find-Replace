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
	public class ReferenceQuery
	{
		private const string PROG_BAR_TITLE_FINDING = "Finding References";

		//Inputs
		public string rootFolderToSearch = "Assets";
		public List<string> extensionsToSearch = new List<string>()
		{
			".prefab",
			".unity",
			".asset",
			".spriteatlas",
		};

		//Outputs
		public List<string> referencingPaths = null;
		
		/// Finds all the references to the object using the default object regex
		public void FindReferences(Object objectToFind) => FindReferences(RegexForAsset(objectToFind));

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
				referencingPaths = new List<string>(RegexFiles(filesToSearch, searchRegex));
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
				
				for(int i = 0; i < referencingPaths.Count; ++i)
				{
					string path = referencingPaths[i];

					if (!EditorUtility.DisplayCancelableProgressBar(PROG_BAR_TITLE_FINDING, path, (float)i / (float)referencingPaths.Count))
					{
						string text = File.ReadAllText(path);
						text = searchRegex.Replace(text, replacement);
						File.WriteAllText(path, text);
					}
					else
					{
						Debug.LogError("[ReferenceQuery] Request cancelled by user");
						break;
					}
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();
				referencingPaths = null;
			}
		}

		private List<FileInfo> FindFilesToSearch()
		{
			DirectoryInfo folderToSearch = new DirectoryInfo(rootFolderToSearch);
			List<FileInfo> filesToSearch = new List<FileInfo>();

			foreach(var extension in extensionsToSearch)
			{
				filesToSearch.AddRange(folderToSearch.GetFiles("*" + extension, SearchOption.AllDirectories));
			}

			return filesToSearch;
		}

		private IEnumerable<string> RegexFiles(List<FileInfo> filesToSearch, Regex regex)
		{
			for(int i = 0; i < filesToSearch.Count; ++i)
			{
				var file = filesToSearch[i];
				string path = file.FullName;

				if (!EditorUtility.DisplayCancelableProgressBar(PROG_BAR_TITLE_FINDING, path, (float)i / (float)filesToSearch.Count))
				{
					string text = File.ReadAllText(path);
					bool isMatch = regex.IsMatch(text);
					
					if (isMatch)
					{
						yield return path;
					}
				}
				else
				{
					Debug.LogError("[ReferenceQuery] Request cancelled by user");
					break;
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
		#endregion
	}
}