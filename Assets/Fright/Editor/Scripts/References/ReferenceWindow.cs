using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fright.Editor.References
{
	public class ReferenceWindow : EditorWindow
	{
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
					query.objectToFind = EditorGUILayout.ObjectField("Object To Find", query.objectToFind, typeof(Object), false);
				}
				if (EditorGUI.EndChangeCheck())
				{
					query.referencingPaths = null;
				}
				EditorGUILayout.Space();
				
				//Draw the object details
				if (query.objectToFind)
				{
					EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
					EditorGUILayout.TextField("Type", query.objectToFind.GetType().Name);
					EditorGUILayout.TextField("GUID", query.guid);
					EditorGUILayout.TextField("Subasset", AssetDatabase.IsMainAsset(query.objectToFind) ? "No" : "Yes");
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
			GUI.enabled = (bool)query.objectToFind;

			if (query?.objectToFind == null || AssetDatabase.IsMainAsset(query.objectToFind))
			{
				if (GUILayout.Button("Search For Asset"))
				{
					query.FindReferences();
				}
			}
			else
			{
				if (GUILayout.Button("Search For Sub Asset"))
				{
					query.FindReferences(ReferenceQuery.RegexForSubAsset(query.objectToFind));
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