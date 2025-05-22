using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using HeroicEngine.Enums;
using HeroicEngine.Utils.Pooling;

namespace HeroicEngine.Systems.ScenesManagement
{
    public class ScenesLoader : SystemBase
    {
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private IMusicPlayer _musicPlayer;
        [Inject] private IUIController _uiController;
        [Inject] private ITimeManager _timeManager;

        private bool _sceneLoading;

        protected override void Start()
        {
            base.Start();
            _localizationManager.ResolveTexts();
            _musicPlayer.Play(MusicEntryType.MainMenu);
        }

        public void ToMainMenu()
        {
            _uiController.HideUIParts(UIPartType.ExitButton);
            LoadSceneAsync(mainMenuSceneName, () => { _uiController.ShowUIParts(UIPartType.MainMenuButtons); });
        }

        public bool IsSceneLoading()
        {
            return _sceneLoading;
        }

        public void RestartScene()
        {
            _uiController.HideUIParts(UIPartType.FailScreen);
            LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        public void LoadSceneAsync(string name)
        {
            LoadSceneAsync(name, null);
        }

        public void LoadSceneAsync(string name, Action callback)
        {
            if (_sceneLoading)
            {
                return;
            }
            _musicPlayer.Stop();
            PoolSystem.ResetPools();
            var asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            StartCoroutine(LevelLoader(asyncLoad, callback));
        }

        private IEnumerator LevelLoader(AsyncOperation asyncLoad, Action callback)
        {
            _timeManager.ResumeGame();

            _sceneLoading = true;

            _uiController.HideUIParts(UIPartType.MainMenuButtons);
            _uiController.ShowUIParts(UIPartType.LoadingScreen);

            asyncLoad.allowSceneActivation = false;
            var smoothProgress = 0f;

            while (!asyncLoad.isDone)
            {
                var targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.deltaTime * 5f);
                _uiController.UpdateLoadingPanel(smoothProgress, "Loading");

                // Scene Activation Condition (Example: Wait for full load and press a key)
                if (smoothProgress >= 0.99f)
                {
                    _uiController.UpdateLoadingPanel(1f, _localizationManager.GetLocalizedString("PressAnyKey"));
                    if (Input.anyKeyDown)
                        asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            callback?.Invoke();

            _uiController.HideUIParts(UIPartType.LoadingScreen);

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                _uiController.ShowUIParts(UIPartType.ExitButton);
            }

            if (SceneManager.GetActiveScene().name == "MainMenuScene")
            {
                _musicPlayer.Play(MusicEntryType.MainMenu);
            }

            _sceneLoading = false;
        }
    }
}
