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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fright.Editor.Templates
{
	[System.Serializable]
	public class BuildOption
	{
		public string id;
		public string name;
		public string textValue;
		public bool isRequired;

		public virtual bool isRequirementMet
		{
			get { return !string.IsNullOrEmpty(textValue); }
		}

		public BuildOption(XmlBuildOption xmlBuildOption)
		{
			id = xmlBuildOption.id;
			name = xmlBuildOption.name;
			isRequired = xmlBuildOption.isRequired;
			SetTextValue(xmlBuildOption.@default);
		}

		public virtual void DoLayout()
		{
			textValue = EditorGUILayout.TextField(name, textValue);
		}

		public virtual void SetTextValue(string textValue)
		{
			this.textValue = textValue;
		}
	}

	public class IntBuildOption : BuildOption
	{
		public int intValue;

		public IntBuildOption(XmlBuildOption xmlBuildOption) : base(xmlBuildOption)
		{
			//...
		}

		public override void DoLayout()
		{
			EditorGUI.BeginChangeCheck();
			{
				intValue = EditorGUILayout.IntField(name, intValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				textValue = intValue.ToString();
			}
		}

		public override void SetTextValue(string textValue)
		{
			base.SetTextValue(textValue);
			int.TryParse(textValue, out intValue);
		}
	}

	public class FloatBuildOption : BuildOption
	{
		public float floatValue;

		public FloatBuildOption(XmlBuildOption xmlBuildOption) : base(xmlBuildOption)
		{
			//...
		}

		public override void DoLayout()
		{
			EditorGUI.BeginChangeCheck();
			{
				floatValue = EditorGUILayout.FloatField(name, floatValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				textValue = floatValue.ToString();
			}
		}

		public override void SetTextValue(string textValue)
		{
			base.SetTextValue(textValue);
			float.TryParse(textValue, out floatValue);
		}
	}

	public class BoolBuildOption : BuildOption
	{
		public bool boolValue;

		public BoolBuildOption(XmlBuildOption xmlBuildOption) : base(xmlBuildOption)
		{
			//...
		}

		public override void DoLayout()
		{
			EditorGUI.BeginChangeCheck();
			{
				boolValue = EditorGUILayout.Toggle(name, boolValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				textValue = boolValue.ToString();
			}
		}

		public override void SetTextValue(string textValue)
		{
			base.SetTextValue(textValue);
			bool.TryParse(textValue, out boolValue);
		}
	}
}