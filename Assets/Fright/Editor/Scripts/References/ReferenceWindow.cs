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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fright.Editor.References
{
	public class ReferenceWindow : EditorWindow
	{
		public const string WARNING_MULTIPLE_REPLACE = "You're replacing references to multiple assets with a single reference.";
		public const string WARNING_RESET_TITLE = "Reset Settings";
		public const string WARNING_RESET_BODY = "Are you sure you want to reset your settings to their default value?";
		public const string WARNING_EXTENSIONS_TIME = "Changing the extensions searched will affect how long the search operation takes.";

		public List<Object?> ObjectsToFind = new List<Object?>() { null };
		public Object? ObjectToReplace;
		public string RegexToFind = string.Empty;
		public string RegexToReplace = string.Empty;
		public Vector2 ScrollPosition = default;
		public Tab CurrentTabView = Tab.Asset;

		[SerializeField] private ReferenceQuery Query = new ReferenceQuery();
		[SerializeField] private QueryWindowSettings Settings = new QueryWindowSettings();
		private SerializedObject? SerializedObject = null;

		public int CountSearchableAssets
		{
			get
			{
				int result = 0;

				foreach (var asset in ObjectsToFind)
				{
					if (asset)
					{
						++result;
					}
				}

				return result;
			}
		}
		
		[MenuItem("Window/Asset References")]
		public static void OpenWindow() => GetWindow<ReferenceWindow>(true, "References").ShowUtility();

		[MenuItem("Assets/Find References", true)]
		public static bool CanOpenWindowWithSelectedObject() => (bool)Selection.activeObject;

		[MenuItem("Assets/Find References")]
		public static void OpenWindowWithSelectedObject()
		{
			var window = GetWindow<ReferenceWindow>(true, "References");
			window.ShowUtility();
			window.ObjectsToFind = new List<Object?>(Selection.objects);
			
			if (window.ObjectsToFind.Count > 0)
			{
				window.BuildFindRegex();
				window.StartSearch();
			}
		}

		private void OnEnable()
		{
			SerializedObject = new SerializedObject(this);
			Settings = QueryWindowSettings.LoadFromEditorPrefs();
			Query.Settings = Settings;
			BuildFindRegex();
		}

		private void OnDisable()
		{
			Settings.SaveToEditorPrefs();
		}

		public void OnGUI()
		{
			titleContent.text = "References";
			SerializedObject!.UpdateIfRequiredOrScript();

			DrawToolbar();

			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			{
				switch(CurrentTabView)
				{
					case Tab.Asset:
						DrawObjectToSelect();
						DrawSearchPanel();
						DrawReferences();
						break;

					case Tab.Regex:
						DrawRegex();
						DrawSearchPanel();
						DrawReferences();
						break;

					case Tab.Settings:
						DrawSettings();
						break;
				}
			}
			EditorGUILayout.EndScrollView();
			SerializedObject.ApplyModifiedProperties();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				if (GUILayout.Button("Find Asset(s)", EditorStyles.toolbarButton))
				{
					CurrentTabView = Tab.Asset;
				}

				if (GUILayout.Button("Find Regex", EditorStyles.toolbarButton))
				{
					CurrentTabView = Tab.Regex;
				}

				if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup@2x"), EditorStyles.toolbarButton, GUILayout.Width(40.0f)))
				{
					CurrentTabView = Tab.Settings;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawObjectToSelect()
		{
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Find assets", EditorStyles.boldLabel);
				EditorGUILayout.Space();

				//Draw the find objects selector
				EditorGUI.BeginChangeCheck();
				{
					EditorGUILayout.PropertyField(SerializedObject!.FindProperty(nameof(ObjectsToFind)), true);
				}
				if (EditorGUI.EndChangeCheck())
				{
					OnChangedObjectsToSelect();
				}

				//Draw the replace object selector
				if (Settings.ShouldAllowReplace)
				{
					DrawObjectToReplace();
				}
				
				//Draw the call to action
				if (CountSearchableAssets == 0)
				{
					EditorGUILayout.LabelField("Pick an asset to get started");
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void OnChangedObjectsToSelect()
		{
			SerializedObject!.ApplyModifiedProperties();

			if (CountSearchableAssets > 1)
			{
				ObjectToReplace = null;
				BuildReplaceRegex();
			}

			Query.ReferencingPaths = new List<string>();
			BuildFindRegex();
		}

		private void DrawObjectToReplace()
		{
			EditorGUI.BeginChangeCheck();
			{
				ObjectToReplace = EditorGUILayout.ObjectField("Object To Replace", ObjectToReplace, typeof(Object), false);
			}
			if (EditorGUI.EndChangeCheck())
			{
				BuildReplaceRegex();
			}

			EditorGUILayout.Space();

			//Draw the multiple object replace warning
			if (CountSearchableAssets >= 2 && !string.IsNullOrEmpty(RegexToReplace))
			{
				EditorGUILayout.HelpBox(WARNING_MULTIPLE_REPLACE, MessageType.Warning);
			}
		}

		private void DrawRegex()
		{
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Find with regular expression", EditorStyles.boldLabel);
				EditorGUILayout.Space();

				RegexToFind = EditorGUILayout.TextField("Find Regex", RegexToFind);

				if (Settings.ShouldAllowReplace)
				{
					RegexToReplace = EditorGUILayout.TextField("Replace Regex", RegexToReplace);
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawSettings()
		{
			var extensionsToSearch = SerializedObject!.FindProperty(nameof(Settings));
			EditorGUILayout.PropertyField(extensionsToSearch);

			EditorGUILayout.Space();
			if (GUILayout.Button("Reset Settings To Default") && EditorUtility.DisplayDialog(WARNING_RESET_TITLE, WARNING_RESET_BODY, "Yes", "No"))
			{
				Settings = new QueryWindowSettings();
				Query.Settings = Settings;
			}
		}

		private void BuildFindRegex()
		{
			if (CountSearchableAssets > 0)
			{
				RegexToFind = ReferenceQuery.RegexForAssets(ObjectsToFind);
			}
			else
			{
				RegexToFind = string.Empty;
			}
		}

		private void BuildReplaceRegex()
		{
			if (!ObjectToReplace)
			{
				RegexToReplace = string.Empty;
				return;
			}

			RegexToReplace = AssetDatabase.IsMainAsset(ObjectToReplace) ? ReferenceQuery.RegexForAsset(ObjectToReplace!) : ReferenceQuery.RegexForSubAsset(ObjectToReplace!);
		}

		private void DrawSearchPanel()
		{
			GUI.enabled = !string.IsNullOrEmpty(RegexToFind);
			EditorGUILayout.BeginHorizontal();

			if (Settings.ShouldAllowReplace)
			{
				if (GUILayout.Button("Find", EditorStyles.miniButtonLeft))
				{
					StartSearch();
				}

				GUI.enabled &= Query.ReferencingPaths?.Count > 0 && !string.IsNullOrEmpty(RegexToReplace);

				if (GUILayout.Button("Replace", EditorStyles.miniButtonRight) && PromptReplace())
				{
					Query.ReplaceReferences(RegexToFind, RegexToReplace);
				}
			}
			else
			{
				if (GUILayout.Button("Find"))
				{
					StartSearch();
				}
			}
			
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
		}

		private void StartSearch()
		{
			Query.FindReferences(RegexToFind);
		}

		private void DrawReferences()
		{
			if (Query?.ReferencingPaths != null)
			{
				EditorGUILayout.BeginVertical("box");
				{
					EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

					if (Query.ReferencingPaths.Count == 0)
					{
						EditorGUILayout.LabelField("No references");
					}
					else
					{
						var currentDirectory = new System.Uri(System.Environment.CurrentDirectory + "/Assets");

						foreach(var path in Query.ReferencingPaths)
						{
							EditorGUILayout.BeginHorizontal();
							{
								string relativePath = currentDirectory.MakeRelativeUri(new System.Uri(path)).ToString();
								relativePath = System.Uri.UnescapeDataString(relativePath);
								EditorGUILayout.LabelField(relativePath);

								if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(60.0f)))
								{
									Object obj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
									Selection.activeObject = obj;
								}
							}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
		}

		private bool PromptReplace()
		{
			return !EditorUtility.DisplayDialog("Find and Replace", "Are you sure you want to replace all references of this asset? This process cannot be undone", "No", "Yes");
		}

		#region Embedded Types
		public enum Tab
		{
			Asset = 0,
			Regex = 1,
			Settings = 2,
		}
		#endregion
	}
}