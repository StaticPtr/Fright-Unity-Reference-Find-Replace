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

		public List<Object> objectsToFind = new List<Object>() { null };
		public Object objectToReplace;
		public string regexToFind;
		public string regexToReplace;
		public Vector2 scrollPosition;
		public int currentTabView = (int)Tab.asset;

		private ReferenceQuery query = new ReferenceQuery();
		private SerializedObject serializedObject = null;
		private string[] tabs = new string[] {"Asset", "Regex"};

		public int searchableAssets
		{
			get
			{
				int result = 0;

				foreach(var asset in objectsToFind)
				{
					if (asset) ++result;
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
			window.objectsToFind = new List<Object>(Selection.objects);
			
			if (window.objectsToFind.Count > 0)
			{
				window.BuildFindRegex();
				window.StartSearch();
			}
		}

		private void OnEnable()
		{
			serializedObject = new SerializedObject(this);
			BuildFindRegex();
		}

		public void OnGUI()
		{
			titleContent.text = "References";
			serializedObject.UpdateIfRequiredOrScript();

			DrawToolbar();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			{
				switch((Tab)currentTabView)
				{
					case Tab.asset:
						DrawObjectToSelect();
						break;

					case Tab.regex:
						DrawRegex();
						break;
				}

				DrawSearchPanel();
				DrawReferences();
			}
			EditorGUILayout.EndScrollView();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				currentTabView = GUILayout.SelectionGrid(currentTabView, tabs, tabs.Length, EditorStyles.toolbarButton);
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawObjectToSelect()
		{
			EditorGUILayout.BeginVertical("box");
			{
				//Draw the find objects selector
				EditorGUI.BeginChangeCheck();
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(objectsToFind)), true);
				}
				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();

					if (searchableAssets > 1)
					{
						objectToReplace = null;
						BuildReplaceRegex();
					}

					query.referencingPaths = null;
					BuildFindRegex();
				}

				//Draw the replace object selector
				EditorGUI.BeginChangeCheck();
				{
					objectToReplace = EditorGUILayout.ObjectField("Object To Replace", objectToReplace, typeof(Object), false);
				}
				if (EditorGUI.EndChangeCheck())
				{
					BuildReplaceRegex();
				}

				EditorGUILayout.Space();

				//Draw the multiple object replace warning
				if (searchableAssets >= 2 && !string.IsNullOrEmpty(regexToReplace))
				{
					EditorGUILayout.HelpBox(WARNING_MULTIPLE_REPLACE, MessageType.Warning);
				}
				
				//Draw the call to action
				if (searchableAssets == 0)
				{
					EditorGUILayout.LabelField("Pick an asset to get started");
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawRegex()
		{
			EditorGUILayout.BeginVertical("box");
			{
				regexToFind = EditorGUILayout.TextField("Find", regexToFind);
				regexToReplace = EditorGUILayout.TextField("Replace", regexToReplace);
			}
			EditorGUILayout.EndVertical();
		}

		private void BuildFindRegex()
		{
			if (searchableAssets > 0)
			{
				regexToFind = ReferenceQuery.RegexForAssets(objectsToFind);
			}
			else
			{
				regexToFind = "";
			}
		}

		private void BuildReplaceRegex()
		{
			regexToReplace = AssetDatabase.IsMainAsset(objectToReplace) ? ReferenceQuery.RegexForAsset(objectToReplace) : ReferenceQuery.RegexForSubAsset(objectToReplace);
		}

		private void DrawSearchPanel()
		{
			GUI.enabled = !string.IsNullOrEmpty(regexToFind);

			if (GUILayout.Button("Find"))
			{
				StartSearch();
			}

			GUI.enabled &= query.referencingPaths?.Count > 0 && !string.IsNullOrEmpty(regexToReplace);

			if (GUILayout.Button("Replace") && PromptReplace())
			{
				query.ReplaceReferences(regexToFind, regexToReplace);
			}
			
			GUI.enabled = true;
		}

		private void StartSearch()
		{
			query.FindReferences(regexToFind);
		}

		private void DrawReferences()
		{
			if (query?.referencingPaths != null)
			{
				EditorGUILayout.BeginVertical("box");
				{
					EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

					if (query.referencingPaths.Count == 0)
					{
						EditorGUILayout.LabelField("No references");
					}
					else
					{
						var currentDirectory = new System.Uri(System.Environment.CurrentDirectory + "/Assets");

						foreach(var path in query.referencingPaths)
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
			asset = 0,
			regex = 1,
		}
		#endregion
	}
}