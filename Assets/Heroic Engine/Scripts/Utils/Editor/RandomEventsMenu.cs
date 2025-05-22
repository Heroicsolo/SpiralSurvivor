using HeroicEngine.Gameplay;
using System.IO;
using UnityEditor;
using UnityEngine;
using HeroicEngine.Utils.Data;

namespace HeroicEngine.Utils.Editor
{
    public sealed class RandomEventsMenu : EditorWindow
    {
        private const string EVENTS_COLLECTION_PATH = "Assets/Heroic Engine/Scriptables/RandomEvents/RandomEventsCollection.asset";
        private const string RANDOM_EVENTS_PATH = "Assets/Heroic Engine/Scriptables/RandomEvents/";

        private GUIStyle _smallInfoStyle;
        private GUIStyle _italicStyle;
        
        private RandomEventsCollection _randomEventsCollection;
        private string _newEventTypeStr = "";
        private float _newEventChance;
        private bool _badLuckProtection = true;
        private bool _goodLuckProtection = true;
        private string _statusText = "";
        private AudioClip _audioClip;

        [MenuItem("Tools/HeroicEngine/Edit Random Events")]
        public static void ShowWindow()
        {
            var window = GetWindow<RandomEventsMenu>("Random Events Editor");
            window.position = new Rect(100, 100, 400, 300);
            window.Show();
        }
        
        private void OnEnable()
        {
            minSize = new Vector2(300, 200);
        }

        private void OnGUI()
        {
            _randomEventsCollection ??= AssetDatabase.LoadAssetAtPath<RandomEventsCollection>(EVENTS_COLLECTION_PATH);

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
                        if (!_randomEventsCollection)
                        {
                            var collection = CreateInstance<RandomEventsCollection>();
                            var randomEventInfo = collection.CreateItem($"{RANDOM_EVENTS_PATH}{_newEventTypeStr}.asset",
                                _newEventTypeStr, _newEventChance, _badLuckProtection, _goodLuckProtection, _audioClip);
                            _randomEventsCollection = collection;
                            randomEventInfo.Initialize();
                        }
                        else
                        {
                            var randomEventInfo = _randomEventsCollection.CreateItem($"{RANDOM_EVENTS_PATH}{_newEventTypeStr}.asset",
                                _newEventTypeStr, _newEventChance, _badLuckProtection, _goodLuckProtection, _audioClip);
                            randomEventInfo.Initialize();
                        }
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
