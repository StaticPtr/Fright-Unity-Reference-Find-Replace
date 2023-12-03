using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fright.Editor.References
{
	[Serializable]
	public class QueryWindowSettings
	{
		public const string EDITOR_PREFS_KEY = "QueryWindowSettings";

		public bool ShouldAllowReplace = true;
		public string RootFolderToSearch = "Assets";
		public List<string> ExtensionsToSearch = new List<string>()
		{
			".prefab",
			".unity",
			".asset",
			".spriteatlas",
		};

		public void SaveToEditorPrefs()
		{
			string json = JsonUtility.ToJson(this);
			EditorPrefs.SetString(EDITOR_PREFS_KEY, json);
		}

		public static QueryWindowSettings LoadFromEditorPrefs()
		{
			string json = EditorPrefs.GetString(EDITOR_PREFS_KEY);

			if (string.IsNullOrEmpty(json))
				return new QueryWindowSettings();

			return JsonUtility.FromJson<QueryWindowSettings>(json);
		}
	}
}