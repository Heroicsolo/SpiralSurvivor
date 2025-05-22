using HeroicEngine.Enums;
using HeroicEngine.Systems.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(UIController))]
    public sealed class UIControllerEditor : UnityEditor.Editor
    {
        private const string FILE_PATH = "Assets/Heroic Engine/Scripts/Enums/";
        private const string FILE_NAME = "UIPartType";

        private GUIStyle _italicStyle;

        private List<string> _uiPartsNames = new();
        private string _newPartName = "";
        private string _statusText = "";

        private void OnEnable()
        {
            _uiPartsNames = new List<string>(Enum.GetNames(typeof(UIPartType)));
        }

        private bool IsPartNameValid()
        {
            return !string.IsNullOrEmpty(_newPartName) && char.IsLetter(_newPartName[0]) && !_newPartName.Contains(" ");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Register new UI Part type");
            _newPartName = EditorGUILayout.TextField(_newPartName);
            
            _italicStyle ??= new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };
            
            if (GUILayout.Button("Save"))
            {
                if (!IsPartNameValid())
                {
                    _statusText = "Incorrect UI Part name!".ToColorizedString(Color.red);
                }
                else if (_uiPartsNames.Contains(_newPartName))
                {
                    _statusText = "This UI Part name is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    _uiPartsNames.Add(_newPartName);
                    EnumUtils.WriteToEnum(FILE_PATH, FILE_NAME, _uiPartsNames);
                    _statusText = "UI Part name registered.".ToColorizedString(Color.green);
                    _newPartName = "";
                }
            }

            EditorGUILayout.LabelField(_statusText, _italicStyle);
        }
    }
}
