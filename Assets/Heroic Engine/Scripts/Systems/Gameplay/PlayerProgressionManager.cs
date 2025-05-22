using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroicEngine.Gameplay
{
    public sealed class PlayerProgressionManager : SystemBase, IPlayerProgressionManager
    {
        private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";
        private const string LEVEL_UP_EVENT_NAME = "LevelUp";
        private const string EXP_CHANGED_EVENT_NAME = "ExpChanged";
        private const string PLAYER_PROGRESSION_STATE_KEY = "PlayerProgression";

        [Header("Progression Params")]
        [SerializeField] private PlayerProgressionParams playerProgressionParams;

        [Inject] private IEventsManager _eventsManager;
        [Inject] private ISoundsManager _soundsManager;
        [Inject] private IUIController _uiController;

        private ProgressionState _playerSaves;
        private int _expPerCurrentLevel;

        public void ResetState()
        {
            _playerSaves = new ProgressionState
            {
                currentLevel = 1, currentExp = 0
            };

            OnExpChanged();

            SaveState();
        }

        public (int, int, int) GetPlayerLevelState()
        {
            return (_playerSaves.currentLevel, _playerSaves.currentExp, GetNeededExpForLevelUp());
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public int GetExpPerCurrentLevel()
        {
            return _expPerCurrentLevel;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (SceneManager.GetActiveScene().name == MAIN_MENU_SCENE_NAME)
            {
                return;
            }

            _expPerCurrentLevel = 0;
        }

        public void AddExperience(int amount)
        {
            _playerSaves.currentExp += amount;

            _expPerCurrentLevel += amount;

            var neededExp = GetCurrentLevelMaxExp();

            var expTotal = _playerSaves.currentExp;
            var lvlChanged = false;

            while (expTotal >= neededExp)
            {
                _playerSaves.currentLevel++;
                lvlChanged = true;
                expTotal -= neededExp;
                neededExp = GetCurrentLevelMaxExp();
            }

            _playerSaves.currentExp = expTotal;

            if (lvlChanged)
            {
                _eventsManager.TriggerEvent(LEVEL_UP_EVENT_NAME, _playerSaves.currentLevel);

                if (playerProgressionParams.LevelUpSound != null)
                {
                    _soundsManager.PlayClip(playerProgressionParams.LevelUpSound);
                }
            }

            _eventsManager.TriggerEvent(EXP_CHANGED_EVENT_NAME, _playerSaves.currentExp, GetNeededExpForLevelUp());
            OnExpChanged();

            SaveState();
        }

        public int GetNeededExpForLevelUp()
        {
            return GetCurrentLevelMaxExp() - _playerSaves.currentExp;
        }

        public int GetCurrentLevelMaxExp()
        {
            return Mathf.CeilToInt(playerProgressionParams.BaseExpForLevel * (1f + playerProgressionParams.ExpForLevelMultCoef * Mathf.Pow(_playerSaves.currentLevel, playerProgressionParams.ExpForLevelDegreeCoef)));
        }

        private void OnExpChanged()
        {
            _uiController.UpdateExperiencePanel(_playerSaves.currentLevel, _playerSaves.currentExp, GetCurrentLevelMaxExp());
        }

        private void LoadState()
        {
            var playerSavesString = PlayerPrefs.GetString(PLAYER_PROGRESSION_STATE_KEY, "");

            if (!string.IsNullOrEmpty(playerSavesString))
            {
                _playerSaves = JsonUtility.FromJson<ProgressionState>(playerSavesString);
            }
            else
            {
                _playerSaves = new ProgressionState
                {
                    currentLevel = 1, currentExp = 0
                };
            }
        }

        private void SaveState()
        {
            var playerSavesString = JsonUtility.ToJson(_playerSaves);
            PlayerPrefs.SetString(PLAYER_PROGRESSION_STATE_KEY, playerSavesString);
        }

        private void Awake()
        {
            LoadState();
        }

        protected override void Start()
        {
            base.Start();
            OnExpChanged();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SaveState();
        }
    }

    [Serializable]
    public struct ProgressionState
    {
        public int currentExp;
        public int currentLevel;
    }
}
