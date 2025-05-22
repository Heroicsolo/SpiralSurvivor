using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(RandomEventsManager))]
    public sealed class RandomEventsEditor : UnityEditor.Editor
    {
        private GUIStyle _smallInfoStyle;
        private GUIStyle _italicStyle;
        
        private RandomEventsManager _myScript;
        private string _newEventTypeStr = "";
        private float _newEventChance;
        private bool _badLuckProtection = true;
        private bool _goodLuckProtection = true;
        private string _statusText = "";
        private AudioClip _audioClip;

        private void OnEnable()
        {
            _myScript = (RandomEventsManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            _smallInfoStyle ??= new GUIStyle
            {
                fontStyle = FontStyle.Italic, fontSize = 11,
                wordWrap = true, richText = true
            };
            
            _italicStyle ??= new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };

            EditorGUILayout.LabelField("Register new random event");
            EditorGUILayout.LabelField("Event name:");
            _newEventTypeStr = EditorGUILayout.TextField(_newEventTypeStr);
            EditorGUILayout.LabelField("Event chance:");
            _newEventChance = EditorGUILayout.Slider(_newEventChance, 0f, 1f);
            EditorGUILayout.LabelField("Bad luck protection:");
            EditorGUILayout.LabelField("If true, system will guarantee that event with 1/N chance will occur in less than N+1 attempts\nOtherwise, it will be pure random"
                .ToColorizedString(Color.white), _smallInfoStyle);
            _badLuckProtection = EditorGUILayout.Toggle(_badLuckProtection);
            if (_badLuckProtection)
            {
                EditorGUILayout.LabelField("Good luck protection:");
                EditorGUILayout.LabelField("If true, system will decrease event chance when it occurs, the earlier it occurs - the bigger that decrease"
                    .ToColorizedString(Color.white), _smallInfoStyle);
                _goodLuckProtection = EditorGUILayout.Toggle(_goodLuckProtection);
            }
            else
            {
                _goodLuckProtection = false;
            }

            _audioClip = (AudioClip)EditorGUILayout.ObjectField("Event sound", _audioClip, typeof(AudioClip), false);
            
            if (GUILayout.Button("Register"))
            {
                if (!string.IsNullOrEmpty(_newEventTypeStr))
                {
                    if (!File.Exists($"{Application.dataPath}/Heroic Engine/Scriptables/RandomEvents/{_newEventTypeStr}.asset"))
                    {
                        var asset = CreateInstance<RandomEventInfo>();
                        asset.Construct(_newEventTypeStr, _newEventChance, _badLuckProtection, _goodLuckProtection, _audioClip);
                        AssetDatabase.CreateAsset(asset, $"Assets/Heroic Engine/Scriptables/RandomEvents/{_newEventTypeStr}.asset");
                        AssetDatabase.SaveAssets();
                        _myScript.RegisterEvent(asset);
                        _statusText = "Random event registered.".ToColorizedString(Color.green);
                    }
                    else
                    {
                        _statusText = "This event was already registered!".ToColorizedString(Color.red);
                    }
                }
            }

            EditorGUILayout.LabelField(_statusText, _italicStyle);
        }
    }
}