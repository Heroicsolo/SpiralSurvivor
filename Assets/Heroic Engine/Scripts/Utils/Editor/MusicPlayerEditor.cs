using HeroicEngine.Enums;
using HeroicEngine.Systems.Audio;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(MusicPlayer))]
    public sealed class MusicPlayerEditor : UnityEditor.Editor
    {
        private const string ENUM_PATH = "Assets/Heroic Engine/Scripts/Enums/";
        private const string ENUM_NAME = "MusicEntryType";

        private string _musicEntryName;
        private List<string> _musicEntriesNames;
        private string _statusText;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _musicEntriesNames = new List<string>(Enum.GetNames(typeof(MusicEntryType)));

            EditorGUILayout.BeginVertical(GUI.skin.box);

            var headerStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold, richText = true
            };

            EditorGUILayout.LabelField("New music entries registration".ToColorizedString(Color.white), headerStyle);

            _musicEntryName = EditorGUILayout.TextField("Music entry name", _musicEntryName);

            if (GUILayout.Button("Add Music Entry"))
            {
                if (!IsEntryNameValid())
                {
                    _statusText = "Incorrect entry type name!".ToColorizedString(Color.red);
                }
                else if (_musicEntriesNames.Contains(_musicEntryName))
                {
                    _statusText = "This entry is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    _musicEntriesNames.Add(_musicEntryName);
                    EnumUtils.WriteToEnum(ENUM_PATH, ENUM_NAME, _musicEntriesNames);
                    _statusText = "Music entry type registered.".ToColorizedString(Color.green);
                    _musicEntryName = "";
                }
            }

            var italicStyle = new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };

            EditorGUILayout.LabelField(_statusText, italicStyle);

            EditorGUILayout.EndVertical();
        }

        private bool IsEntryNameValid()
        {
            return !string.IsNullOrEmpty(_musicEntryName) && char.IsLetter(_musicEntryName[0]) && !_musicEntryName.Contains(" ");
        }
    }
}
