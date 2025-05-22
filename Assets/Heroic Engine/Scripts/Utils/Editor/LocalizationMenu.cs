using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public sealed class LocalizationMenu : EditorWindow
    {
        private const char SEPARATOR = '=';
        
        private readonly Dictionary<SystemLanguage, Dictionary<string, string>> _translations = new();
        private readonly List<SystemLanguage> _availableLanguages = new();

        private GUIStyle _italicStyle;
        private int _selectedKey;
        private int _prevKey;
        private int _selectedLang;
        private string _localizationKey = "";
        private string _localizationKey2 = "";
        private readonly Dictionary<SystemLanguage, string> _localizedValues = new();
        private readonly Dictionary<SystemLanguage, string> _localizedValues2 = new();
        private string _statusText;

        [MenuItem("Tools/HeroicEngine/Edit Localizations")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationMenu>("Localization Editor");
        }

        private void OnGUI()
        {
            LoadTranslations();

            GUILayout.Label("Localization Key:");
            _localizationKey = GUILayout.TextField(_localizationKey);

            foreach (var lang in _availableLanguages)
            {
                GUILayout.Label($"Translation ({lang}):");
                _localizedValues.TryAdd(lang, "");
                _localizedValues[lang] = GUILayout.TextField(_localizedValues[lang]);
            }

            if (GUILayout.Button("Add Translation"))
            {
                if (!string.IsNullOrEmpty(_localizationKey))
                {
                    foreach (var lang in _availableLanguages)
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
                GUILayout.Label("Localization Key:");
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

                foreach (var lang in _availableLanguages)
                {
                    GUILayout.Label($"Translation ({lang}):");
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
                    foreach (var lang in _availableLanguages)
                    {
                        AddTranslation(lang, _localizationKey2, _localizedValues2[lang]);
                        SaveTranslationsForLang(lang);
                    }
                }

                EditorGUILayout.Separator();
            }
            GUILayout.Label("Add localization language:");

            var allLanguages = new List<string>(Enum.GetNames(typeof(SystemLanguage)));

            _availableLanguages.ForEach(al => allLanguages.Remove(Enum.GetName(typeof(SystemLanguage), al)));

            _selectedLang = EditorGUILayout.Popup(_selectedLang, allLanguages.ToArray());
            var newLanguage = (SystemLanguage)_selectedLang;

            if (GUILayout.Button("Add Language"))
            {
                AddNewLanguage(newLanguage);
            }
        }

        private void AddNewLanguage(SystemLanguage lang)
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
            _availableLanguages.Clear();

            var files = Resources.LoadAll<TextAsset>("Localization");

            for (var i = 0; i < files.Length; i++)
            {
                var fileLanguage = SystemLanguage.English;

                foreach (var line in files[i].text.Split('\n'))
                {
                    if (line.Contains("#"))
                    {
                        fileLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), line.Replace("#", ""));
                        if (!_availableLanguages.Contains(fileLanguage))
                        {
                            _availableLanguages.Add(fileLanguage);
                        }
                    }
                    else
                    {
                        if (!_translations.ContainsKey(fileLanguage))
                        {
                            _translations.Add(fileLanguage, new Dictionary<string, string>());
                        }
                        var prop = line.Split(SEPARATOR);
                        _translations[fileLanguage][prop[0]] = prop[1];
                    }
                }
            }
        }

        private void SaveTranslationsForLang(SystemLanguage lang)
        {
            var fileName = Enum.GetName(typeof(SystemLanguage), lang);
            var fileContent = $"#{fileName}";
            var path = Application.dataPath + "/Heroic Engine/Resources/Localization/" + fileName + ".txt";

            foreach (var item in _translations[lang])
            {
                fileContent += $"\n{item.Key}={item.Value}";
            }

            File.WriteAllText(path, fileContent);
        }
    }
}
