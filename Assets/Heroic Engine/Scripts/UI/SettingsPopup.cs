using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.DI;
using System;
using TMPro;
using UnityEngine;

namespace HeroicEngine.UI
{
    public sealed class SettingsPopup : UIPart
    {
        [SerializeField] private TMP_Dropdown languageSelector;

        [Inject] private ILocalizationManager _localizationManager;

        private void Start()
        {
            InjectionManager.InjectTo(this);

            var availableLanguages = _localizationManager.GetAvailableLanguages();
            var languages = availableLanguages
                .ConvertAll((l) => Enum.GetName(typeof(SystemLanguage), l));

            languageSelector.ClearOptions();
            languageSelector.AddOptions(languages);

            languageSelector.value = availableLanguages.FindIndex(l => l == _localizationManager.GetCurrentLanguage());
        }

        public void OnLanguageSelected()
        {
            var selectedLang = languageSelector.value;

            var languages = _localizationManager.GetAvailableLanguages();

            _localizationManager.SwitchLanguage(languages[selectedLang]);
        }
    }
}