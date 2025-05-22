using HeroicEngine.Systems.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationEditor : UnityEditor.Editor
    {
        private const char SEPARATOR = '=';
        
        private LocalizationManager _localizationManager;
        private readonly Dictionary<SystemLanguage, Dictionary<string, string>> _translations = new();
        private readonly Dictionary<SystemLanguage, string> _localizedValues = new();
        private readonly Dictionary<SystemLanguage, string> _localizedValues2 = new();

        private GUIStyle _italicStyle;
        private int _selectedKey;
        private int _prevKey;
        private int _selectedLang;
        private string _localizationKey = "";
        private string _localizationKey2 = "";
        private string _statusText;

        private void OnEnable()
        {
            _localizationManager = (LocalizationManager)target;
        }

        private static void AddNewLanguage(SystemLanguage lang)
        {
            var fileName = Enum.GetName(typeof(SystemLanguage), lang);
            var path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";
            File.WriteAllText(path, $"#{fileName}");
        }

        private void AddTranslation(SystemLanguage lang, string key, string translation)
        {
            if (!_translations.ContainsKey(lang))
            {
                _translations.Add(lang, new Dictionary<string, string>());
            }

            _translations[lang][key] = translation;
        }

        private void LoadTranslations()
        {
            _translations.Clear();

            var availableLanguages = _localizationManager.GetAvailableLanguages();

            foreach (var lang in availableLanguages)
            {
                var fileName = Enum.GetName(typeof(SystemLanguage), lang);
                var path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";

                if (Directory.Exists(path))
                {
                    var lines = File.ReadAllLines(path);

                    foreach (var line in lines)
                    {
                        if (!line.Contains("#"))
                        {
                            if (!_translations.ContainsKey(lang))
                            {
                                _translations.Add(lang, new Dictionary<string, string>());
                            }
                            var prop = line.Split(SEPARATOR);
                            _translations[lang][prop[0]] = prop[1];
                        }
                    }
                }
            }
        }

        private void SaveTranslationsForLang(SystemLanguage lang)
        {
            var fileName = Enum.GetName(typeof(SystemLanguage), lang);
            var fileContent = $"#{fileName}";
            var path = Application.dataPath + "/Resources/Localization/" + fileName + ".txt";

            foreach (var item in _translations[lang])
            {
                fileContent += $"\n{item.Key}={item.Value}";
            }

            File.WriteAllText(path, fileContent);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LoadTranslations();

            EditorGUILayout.LabelField("Localization Key:");
            _localizationKey = EditorGUILayout.TextField(_localizationKey);
            var availableLanguages = _localizationManager.GetAvailableLanguages();

            foreach (var lang in availableLanguages)
            {
                EditorGUILayout.LabelField($"Translation ({lang}):");
                _localizedValues.TryAdd(lang, "");
                _localizedValues[lang] = EditorGUILayout.TextField(_localizedValues[lang]);
            }

            if (GUILayout.Button("Add Translation"))
            {
                if (!string.IsNullOrEmpty(_localizationKey))
                {
                    foreach (var lang in availableLanguages)
                    {
                        AddTranslation(lang, _localizationKey, _localizedValues[lang]);
                        SaveTranslationsForLang(lang);
                    }

                    _statusText = "Translation added.".ToColorizedString(Color.green);
                }
                else
                {
                    _statusText = "Translation key is empty!".ToColorizedString(Color.red);
                }
            }

            _italicStyle ??= new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };

            EditorGUILayout.LabelField(_statusText, _italicStyle);
            EditorGUILayout.Separator();

            if (_translations.Count > 0)
            {
                EditorGUILayout.LabelField("Localization Key:");
                var keys = new List<string>(_translations.ElementAt(0).Value.Keys);
                _selectedKey = EditorGUILayout.Popup(_selectedKey, _translations.ElementAt(0).Value.Keys.ToArray());
                if (_selectedKey != _prevKey)
                {
                    foreach (var lang in _localizedValues2.Keys.ToArray())
                    {
                        _localizedValues2[lang] = "";
                    }
                }
                _prevKey = _selectedKey;
                _localizationKey2 = keys[_selectedKey];

                foreach (var lang in availableLanguages)
                {
                    EditorGUILayout.LabelField($"Translation ({lang}):");
                    _localizedValues2.TryAdd(lang, "");
                    if (!_translations.ContainsKey(lang))
                    {
                        _translations.Add(lang, new Dictionary<string, string>());
                    }

                    var val = !string.IsNullOrEmpty(_localizedValues2[lang]) ? _localizedValues2[lang]
                        : _translations[lang].ContainsKey(_localizationKey2) ? _translations[lang][_localizationKey2] : "";
                    _localizedValues2[lang] = GUILayout.TextField(val);
                }

                if (GUILayout.Button("Save Translation"))
                {
                    foreach (var lang in availableLanguages)
                    {
                        AddTranslation(lang, _localizationKey2, _localizedValues2[lang]);
                        SaveTranslationsForLang(lang);
                    }
                }

                EditorGUILayout.Separator();
            }
            EditorGUILayout.LabelField("Add localization language:");

            var allLanguages = new List<string>(Enum.GetNames(typeof(SystemLanguage)));

            availableLanguages.ForEach(al => allLanguages.Remove(Enum.GetName(typeof(SystemLanguage), al)));

            _selectedLang = EditorGUILayout.Popup(_selectedLang, allLanguages.ToArray());
            var newLanguage = (SystemLanguage)_selectedLang;

            if (GUILayout.Button("Add Language"))
            {
                AddNewLanguage(newLanguage);
            }
        }
    }
}
