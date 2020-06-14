using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fright.Editor.References
{
	public class ReferenceWindow : EditorWindow
	{
		public Object objectToFind;
		public Object objectToReplace;
		private ReferenceQuery query = new ReferenceQuery();
		
		[MenuItem("Fright/References")]
		public static void OpenWindow() => GetWindow<ReferenceWindow>().Show();

		public void OnGUI()
		{
			titleContent.text = "References";
			DrawObjectToSelect();
			DrawSearchPanel();
			DrawReferences();
		}

		private void DrawObjectToSelect()
		{
			EditorGUILayout.BeginVertical("box");
			{
				//Draw the object selector
				EditorGUI.BeginChangeCheck();
				{
					objectToFind = EditorGUILayout.ObjectField("Object To Find", objectToFind, typeof(Object), false);
					objectToReplace = EditorGUILayout.ObjectField("Object To Replace", objectToReplace, typeof(Object), false);
				}
				if (EditorGUI.EndChangeCheck())
				{
					query.referencingPaths = null;
				}
				EditorGUILayout.Space();
				
				//Draw the object details
				if (objectToFind)
				{
					EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
					EditorGUILayout.TextField("Type", objectToFind.GetType().Name);
					EditorGUILayout.TextField("GUID", ReferenceQuery.GetObjectGUID(objectToFind));
					EditorGUILayout.TextField("Subasset", AssetDatabase.IsMainAsset(objectToFind) ? "No" : "Yes");
				}
				else
				{
					EditorGUILayout.LabelField("Pick an asset to get started");
				}
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawSearchPanel()
		{
			GUI.enabled = (bool)objectToFind;

			if (objectToFind == null || AssetDatabase.IsMainAsset(objectToFind))
			{
				if (GUILayout.Button("Search For Asset"))
				{
					query.FindReferences(objectToFind);
				}

				GUI.enabled &= query.referencingPaths?.Count > 0 && objectToReplace;

				if (GUILayout.Button("Replace Asset"))
				{
					query.ReplaceReferences(objectToFind, objectToReplace);
				}
			}
			else
			{
				if (GUILayout.Button("Search For Sub Asset"))
				{
					query.FindReferences(ReferenceQuery.RegexForSubAsset(objectToFind));
				}

				GUI.enabled &= query.referencingPaths?.Count > 0 && objectToReplace;

				if (GUILayout.Button("Replace Asset"))
				{
					query.ReplaceReferences(ReferenceQuery.RegexForSubAsset(objectToFind), ReferenceQuery.RegexForSubAsset(objectToReplace));
				}
			}
			
			GUI.enabled = true;
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
						query.referencingPaths.ForEach(path => EditorGUILayout.LabelField(path));
					}
				}
				EditorGUILayout.EndVertical();
			}
		}
	}
}