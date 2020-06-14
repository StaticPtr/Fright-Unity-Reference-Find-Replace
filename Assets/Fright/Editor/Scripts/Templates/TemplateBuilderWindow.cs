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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fright.Editor.Templates
{
	/// An editor window for creating C# scripts from template files
	public class TemplateBuilderWindow : EditorWindow
	{
		public const string METADATA_FILENAME_PREFIX = "filenamePrefix";
		public const string METADATA_FILENAME_SUFFIX = "filenameSuffix";

		private const float MIN_WIDTH = 450.0f;
		private const float MIN_HEIGHT = 400.0f;
		private const float SETTINGS_PANEL_WIDTH = 300.0f;

		[SerializeField] private string lastKnownCreationPath;
		[SerializeField] private Rect sharedWindowPosition = new Rect(Vector2.zero, new Vector2(700.0f, 400.0f));
		[SerializeField] private Vector2 templatePreviewScrollPos = Vector2.zero;
		[SerializeField] private Vector2 templateSettingsScrollPos = Vector2.zero;
		
		private TemplateSettings templateSettings = new TemplateSettings();
		private List<XmlTemplate> templates = null;
		private XmlTemplate template = null;
		private string codePreview = null;

		private GUIStyle codePreviewStyle;

		/// The path to the currently selected folder in the project view
		public static string templateCreateFolderPath
		{
			get
			{
				string result = null;
				Object selectedObject = Selection.activeObject;
				string selectedObjectPath = selectedObject ? AssetDatabase.GetAssetPath(selectedObject) : null;

				//If there is a selected object then grab the folder that it's in
				if (!string.IsNullOrEmpty(selectedObjectPath))
				{
					if (Directory.Exists(selectedObjectPath))
					{
						result = selectedObjectPath;
					}
					else
					{
						result = Path.GetDirectoryName(selectedObjectPath);
					}
				}

				//Return the result
				return result;
			}
		}

		public string templateCreateFilePath
		{
			get
			{
				string result = null;

				if (lastKnownCreationPath != null)
				{
					result = lastKnownCreationPath + "/";
					result += template.GetMetaData(METADATA_FILENAME_PREFIX, string.Empty);
					result += (templateSettings.GetBuildOptionValue("filename") ?? template.id);
					result += template.GetMetaData(METADATA_FILENAME_SUFFIX, string.Empty);
					result += ".cs";
				}

				return result;
			}
		}

		public bool canCreateTemplate
		{
			get
			{
				bool result = true;
				
				result &= !string.IsNullOrEmpty(templateCreateFilePath) && !File.Exists(templateCreateFilePath);
				result &= template != null;
				result &= templateSettings != null;

				//Check if all of the build options' requirements have been met
				if (result && templateSettings.buildOptions.Count > 0)
				{
					for(int i = 0; i < templateSettings.buildOptions.Count && result; ++i)
					{
						BuildOption buildOption = templateSettings.buildOptions[i];
						result &= !buildOption.isRequired || buildOption.isRequirementMet;	
					}
				}

				//Return the result
				return result;
			}
		}

		private static string lastSelectedTemplateID
		{
			get { return EditorPrefs.GetString("com.fright.templatebuilder.lastSelectedTemplateID"); }
			set { EditorPrefs.SetString("com.fright.templatebuilder.lastSelectedTemplateID", value); }
		}

		/// Checks if the template builder window can be opened
		[MenuItem("Assets/Template Window", true)]
		public static bool CanOpenTemplateBuilder()
		{
			return !string.IsNullOrEmpty(templateCreateFolderPath);
		}

		/// Opens the template builder window
		[MenuItem("Assets/Template Window", false, -1000)]
		public static void OpenTemplateBuilder()
		{
			TemplateBuilderWindow window = EditorWindow.CreateInstance<TemplateBuilderWindow>();
			window.titleContent.text = "C# Templates";

			window.position = window.sharedWindowPosition;
			window.ShowUtility();
		}

		public bool SelectTemplate(string templateID)
		{
			bool didFind = false;

			if (templates != null)
			{
				//Iterate over each template and find the one with the matching ID
				for(int i = 0; i < templates.Count; ++i)
				{
					XmlTemplate template = templates[i];

					if (template.id.Equals(templateID, System.StringComparison.InvariantCultureIgnoreCase))
					{
						SelectTemplate(template);
						didFind = true;
						break;
					}
				}
			}

			//Return the result
			return didFind;
		}

		public void SelectTemplate(XmlTemplate template, bool wipeGlobalSettings = false, bool wipeTemplateSettings = false)
		{
			this.template = template;
			this.codePreview = null;
			this.templateSettings = new TemplateSettings(template);
			this.templateSettings.enableSyntaxHighlighting = true;
			lastSelectedTemplateID = template.id;

			if (!wipeGlobalSettings)
			{
				this.templateSettings.RestorePeristentSettings();
			}

			if (!wipeTemplateSettings)
			{
				this.templateSettings.RestorePeristentSettingsForTemplate(template);
			}

			EditorGUIUtility.editingTextField = false;
		}

		private void ApplyMinimumsToWindow()
		{
			Vector2 size = this.position.size;
			size.x = System.Math.Max(size.x, MIN_WIDTH);
			size.y = System.Math.Max(size.y, MIN_HEIGHT);

			Rect position = this.position;
			position.size = size;
			this.position = position;
		}

		private void RebuildTemplateListIfNecessary()
		{
			if (templates == null)
			{
				templates = new List<XmlTemplate>(TemplateBuilder.FindAllTemplatesInProject().OrderBy(t => t.priority));

				if (templates.Count > 0)
				{
					if (!SelectTemplate(lastSelectedTemplateID))
					{
						SelectTemplate(templates[0]);
					}
				}
			}
		}

		private void CreateFileFromTemplate()
		{
			templateSettings.enableSyntaxHighlighting = false;

			//Create the file
			string sourceCode = TemplateBuilder.BuildCodeFromTemplate(template, templateSettings);
			string filePath = templateCreateFilePath;
			File.WriteAllText(filePath, sourceCode);

			//Open the file
			AssetDatabase.Refresh();
			Object createdFile = AssetDatabase.LoadAssetAtPath<Object>(filePath);
			Selection.activeObject = createdFile;
			AssetDatabase.OpenAsset(createdFile);

			//Close this window
			Close();
		}

		#region Drawing
		/// Draws the UI of the template builder window
		private void OnGUI()
		{
			RebuildTemplateListIfNecessary();
			ApplyMinimumsToWindow();

			EditorGUIUtility.labelWidth = 100.0f;
			sharedWindowPosition = position;

			//Update creation path
			if (!string.IsNullOrEmpty(templateCreateFolderPath))
			{
				lastKnownCreationPath = templateCreateFolderPath;
			}

			//Update the preview
			if (codePreview == null && template != null)
			{
				codePreview = TemplateBuilder.BuildCodeFromTemplate(template, templateSettings);
				codePreview = codePreview.Replace("\t", "    "); // Unity doesn't render tabs very well in the editor
			}

			if (codePreviewStyle == null)
			{
				codePreviewStyle = new GUIStyle("box");
				codePreviewStyle.wordWrap = false;
				codePreviewStyle.alignment = TextAnchor.UpperLeft;
				codePreviewStyle.padding = new RectOffset(6, 6, 6, 6);
				codePreviewStyle.richText = true;

				codePreviewStyle.normal.textColor =
					codePreviewStyle.active.textColor =
					codePreviewStyle.focused.textColor =
					Color.white;

				codePreviewStyle.normal.background = 
					codePreviewStyle.active.background =
					codePreviewStyle.focused.background = 
					AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Fright/Editor/Skins/CodePreviewBackground.png");

				Font firaFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fright/Editor/Skins/FiraCode-Regular.ttf");

				if (firaFont)
				{
					codePreviewStyle.font = firaFont;
					codePreviewStyle.fontSize = 12;
				}
			}

			//Drawing
			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.BeginHorizontal();
				{
					//Template Settings
					EditorGUILayout.BeginVertical(GUILayout.Width(SETTINGS_PANEL_WIDTH), GUILayout.ExpandHeight(true));
					{
						GUI.enabled = !EditorApplication.isCompiling;
						{
							DrawTemplateSettings();
						}
						GUI.enabled = true;
					}
					EditorGUILayout.EndVertical();

					//Template Preview
					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
					{
						DrawTemplatePreview();
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();

				DrawToolbar();
			}
			if (EditorGUI.EndChangeCheck())
			{
				codePreview = null;

				//Apply any peristent template settings
				if (template != null && templateSettings != null)
				{
					templateSettings.SavePersistentSettings(template);
				}
			}
		}

		private void DrawTemplateSettings()
		{
			templateSettingsScrollPos = EditorGUILayout.BeginScrollView(templateSettingsScrollPos);
			{
				EditorGUILayout.Space();

				//Template Specific Options
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				{
					EditorGUILayout.LabelField("Template Settings", EditorStyles.boldLabel);
					DrawTemplatePicker();
					DrawBuildOptions();
					EditorGUILayout.Space();

					//Optional Usings
					EditorGUILayout.LabelField("Using Namespaces", EditorStyles.boldLabel);
					DrawOptionalUsings();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.Space();

						if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(80.0f)))
						{
							templateSettings.optionalUsings.Add(
								new TemplateSettings.OptionalUsing()
								{
									isCustom = true,
									isEnabled = true,
								}
							);
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}
				EditorGUILayout.EndVertical();

				//Global Settings
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				{
					EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
					templateSettings.lineEndings = (TemplateBuilder.LineEndings)EditorGUILayout.EnumPopup("Line Endings", templateSettings.lineEndings);
					templateSettings.tabMode = (TemplateBuilder.TabMode)EditorGUILayout.EnumPopup("Tabs", templateSettings.tabMode);
					EditorGUILayout.Space();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();

			//Bottom buttons
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			{
				EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));

				//Create button
				bool wasGUIEnabled = GUI.enabled;
				GUI.enabled &= canCreateTemplate;
				{
					if (GUILayout.Button("Create"))
					{
						CreateFileFromTemplate();
					}
				}
				GUI.enabled = wasGUIEnabled;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndVertical();
		}

		private void DrawTemplatePicker()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Template");

				if (EditorGUILayout.DropdownButton(new GUIContent(template != null ? template.id : "<select template>"), FocusType.Keyboard))
				{
					int lastPriority = int.MinValue;
					GenericMenu menu = new GenericMenu();

					foreach(var template in templates)
					{
						XmlTemplate _template = template;

						//Check if the template is malformed
						if (_template.isMalformed)
						{
							menu.AddDisabledItem(new GUIContent(template.id));
						}
						else
						{
							//Add a separator
							if (lastPriority != int.MinValue && _template.priority / 100 != lastPriority / 100)
							{
								string path = Path.GetDirectoryName(_template.id + ".t");

								menu.AddSeparator(string.IsNullOrEmpty(path) ? string.Empty : path + "/");
							}

							menu.AddItem(new GUIContent(template.id), false, () => SelectTemplate(_template));
							lastPriority = _template.priority;
						}
					}

					menu.ShowAsContext();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawTemplatePreview()
		{
			Vector2 preferredSize = codePreviewStyle.CalcSize(new GUIContent(codePreview ?? string.Empty));
			templatePreviewScrollPos = EditorGUILayout.BeginScrollView(templatePreviewScrollPos);
			{
				EditorGUILayout.LabelField(GUIContent.none, new GUIContent(codePreview ?? string.Empty), codePreviewStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(preferredSize.x), GUILayout.MinHeight(preferredSize.y));
			}
			EditorGUILayout.EndScrollView();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandHeight(true));
			{
				EditorGUILayout.LabelField(templateCreateFilePath ?? "select a folder in the project view", EditorStyles.boldLabel);

				if (GUILayout.Button("Reset Template Settings", EditorStyles.toolbarButton, GUILayout.Width(140.0f)))
				{
					SelectTemplate(template, wipeTemplateSettings: true);
				}

				if (GUILayout.Button("Reset Global Settings", EditorStyles.toolbarButton, GUILayout.Width(140.0f)))
				{
					SelectTemplate(template, wipeGlobalSettings: true);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawBuildOptions()
		{
			foreach(var buildOption in templateSettings.buildOptions)
			{
				buildOption.DoLayout();
			}
		}

		private void DrawOptionalUsings()
		{
			foreach(var optionalUsing in templateSettings.optionalUsings)
			{
				if (optionalUsing.isCustom)
				{
					DrawCustomOptionalUsing(optionalUsing);
				}
				else
				{
					optionalUsing.isEnabled = EditorGUILayout.ToggleLeft(optionalUsing.id, optionalUsing.isEnabled);
				}
			}
		}

		private void DrawCustomOptionalUsing(TemplateSettings.OptionalUsing optionalUsing)
		{
			EditorGUILayout.BeginHorizontal();
			{
				optionalUsing.isEnabled = EditorGUILayout.Toggle(optionalUsing.isEnabled, GUILayout.Width(12.0f));
				optionalUsing.id = EditorGUILayout.TextField(optionalUsing.id);

				if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(20.0f)))
				{
					templateSettings.optionalUsings.Remove(optionalUsing);
					GUIUtility.ExitGUI();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		#endregion
	}
}