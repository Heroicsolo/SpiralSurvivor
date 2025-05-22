using HeroicEngine.Enums;
using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public sealed class CurrenciesMenu : EditorWindow
    {
        private const string CURRENCIES_COLLECTION_PATH = "Assets/Heroic Engine/Scriptables/Economics/CurrenciesCollection.asset";
        private const string FILE_PATH = "Assets/Heroic Engine/Scripts/Enums/";
        private const string FILE_NAME = "CurrencyType";
        
        private CurrenciesCollection _currenciesCollection;
        private string _currencyName;
        private string _currencyTitle;
        private Sprite _icon;
        private string _statusText;
        private int _initAmount;
        private List<string> _currencyNames = new();

        [MenuItem("Tools/HeroicEngine/Add Currency")]
        public static void ShowWindow()
        {
            GetWindow<CurrenciesMenu>("Register new currency");
        }

        private bool IsCurrencyNameValid()
        {
            return !string.IsNullOrEmpty(_currencyName) && char.IsLetter(_currencyName[0]) && !_currencyName.Contains(" ");
        }

        private void OnGUI()
        {
            _currenciesCollection ??= AssetDatabase.LoadAssetAtPath<CurrenciesCollection>(CURRENCIES_COLLECTION_PATH);

            _currencyNames = new List<string>(Enum.GetNames(typeof(CurrencyType)));

            EditorGUILayout.LabelField("Create new currency");

            var smallInfoStyle = new GUIStyle
            {
                fontStyle = FontStyle.Italic, fontSize = 9,
                wordWrap = true
            };

            EditorGUILayout.LabelField("Currency name:");
            _currencyName = EditorGUILayout.TextField(_currencyName);
            EditorGUILayout.LabelField("Currency title:");
            _currencyTitle = EditorGUILayout.TextField(_currencyTitle);
            
            _icon = (Sprite)EditorGUILayout.ObjectField("Currency icon", _icon, typeof(Sprite), false);

            _initAmount = EditorGUILayout.IntField("Initial amount", _initAmount);

            var italicStyle = new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };

            if (GUILayout.Button("Register"))
            {
                if (!IsCurrencyNameValid())
                {
                    _statusText = "Incorrect currency type name!".ToColorizedString(Color.red);
                }
                else if (_currencyNames.Contains(_currencyName))
                {
                    _statusText = "This currency is already registered!".ToColorizedString(Color.red);
                }
                else
                {
                    _currencyNames.Add(_currencyName);

                    var currencyInfo = new CurrencyInfo
                    {
                        CurrencyType = _currencyName,
                        Icon = _icon,
                        InitialAmount = _initAmount,
                        Title = _currencyTitle,
                    };

                    if (_currenciesCollection == null)
                    {
                        var asset = CreateInstance<CurrenciesCollection>();
                        asset.RegisterCurrency(currencyInfo);
                        AssetDatabase.CreateAsset(asset, CURRENCIES_COLLECTION_PATH);
                        AssetDatabase.SaveAssets();
                        _currenciesCollection = asset;
                    }
                    else
                    {
                        _currenciesCollection.RegisterCurrency(currencyInfo);
                    }

                    EnumUtils.WriteToEnum(FILE_PATH, FILE_NAME, _currencyNames);
                    _statusText = "Currency registered.".ToColorizedString(Color.green);
                    _currencyName = "";
                }
            }

            EditorGUILayout.LabelField(_statusText, italicStyle);
        }
    }
}