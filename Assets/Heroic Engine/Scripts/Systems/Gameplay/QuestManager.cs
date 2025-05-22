using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Systems.Gameplay
{
    public sealed class QuestManager : SystemBase, IQuestManager
    {
        private const string QUESTS_STATE_FILE_NAME = "QuestsState";
        private const string QUEST_STARTED_EVENT = "QuestStarted";
        private const string QUEST_COMPLETED_EVENT = "QuestCompleted";
        private const string QUEST_PROGRESS_MADE = "QuestProgress";

        [SerializeField] private QuestsCollection questsCollection;

        [Inject] private IEventsManager _eventsManager;
        [Inject] private IUIController _uiController;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private ICurrenciesManager _currenciesManager;
        [Inject] private IPlayerProgressionManager _playerProgressionManager;

        private QuestsState _questsState;
        private readonly Dictionary<string, QuestInfo> _currentQuestInfos = new();

        public void RegisterQuest(QuestInfo quest, bool isInitial = false)
        {
            questsCollection.RegisterQuest(quest, isInitial);
        }

        public void StartQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return;
            }

            if (_eventsManager == null)
            {
                InjectionManager.InjectTo(this);
            }

            if (IsQuestActive(questId))
            {
                return;
            }

            var questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                _currentQuestInfos.Add(questId, questInfo);
                _eventsManager?.TriggerEvent(QUEST_STARTED_EVENT, questId);
            }
        }

        public void AddProgress(QuestTaskType questTaskType, int progress)
        {
            var completedQuests = new List<string>();

            _currentQuestInfos.Values.ToList().ForEach(quest =>
            {
                var taskIdx = quest.GetQuestTaskIndex(questTaskType);

                if (taskIdx >= 0)
                {
                    var questIdx = _questsState.QuestsStates.FindIndex(qs => qs.QuestID == quest.ID);

                    var questTask = quest.QuestTasks[taskIdx];

                    if (questIdx >= 0)
                    {
                        var questState = _questsState.QuestsStates[questIdx];
                        questState.QuestProgress[taskIdx] = Mathf.Min(questState.QuestProgress[taskIdx] + progress, questTask.NeededAmount);
                        _questsState.QuestsStates[questIdx] = questState;
                    }
                    else
                    {
                        List<int> questProgress = new();

                        for (var i = 0; i < quest.QuestTasks.Count; i++)
                        {
                            questProgress.Add(0);
                        }

                        questProgress[taskIdx] = Mathf.Min(progress, questTask.NeededAmount);

                        _questsState.QuestsStates.Add(new QuestState
                        {
                            QuestID = quest.ID, QuestProgress = questProgress
                        });
                    }

                    _eventsManager.TriggerEvent(QUEST_PROGRESS_MADE, quest.ID);
                }

                if (IsQuestCompleted(quest))
                {
                    _eventsManager.TriggerEvent(QUEST_COMPLETED_EVENT, quest.ID);

                    ShowQuestCompletePopup(quest);

                    GetQuestRewards(quest);

                    completedQuests.Add(quest.ID);

                    if (quest.NextQuestIds != null && quest.NextQuestIds.Count > 0)
                    {
                        quest.NextQuestIds.ForEach(StartQuest);
                    }
                }
            });

            completedQuests.ForEach(x => _currentQuestInfos.Remove(x));

            SaveState();
        }

        public QuestInfo GetQuestInfo(string questId)
        {
            return questsCollection.QuestInfos.Find(q => q.ID == questId);
        }

        public QuestState GetQuestState(string questId)
        {
            var questIdx = _questsState.QuestsStates.FindIndex(qs => qs.QuestID == questId);

            if (questIdx >= 0)
            {
                return _questsState.QuestsStates[questIdx];
            }

            List<int> questProgress = new();

            var quest = GetQuestInfo(questId);

            for (var i = 0; i < quest.QuestTasks.Count; i++)
            {
                questProgress.Add(0);
            }

            return new QuestState
            {
                QuestID = questId, QuestProgress = questProgress
            };
        }

        public int GetQuestTaskProgress(string questId, QuestTaskType questTaskType)
        {
            var questState = GetQuestState(questId);

            var questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                var taskIdx = questInfo.GetQuestTaskIndex(questTaskType);

                if (taskIdx >= 0)
                {
                    return questState.QuestProgress[taskIdx];
                }
            }

            return 0;
        }

        public bool IsQuestActive(string questId)
        {
            return _currentQuestInfos.ContainsKey(questId);
        }

        public bool IsQuestCompleted(string questId)
        {
            var questInfo = GetQuestInfo(questId);

            if (questInfo != null)
            {
                return IsQuestCompleted(questInfo);
            }

            return false;
        }

        private void GetQuestRewards(QuestInfo quest)
        {
            foreach (var reward in quest.CurrencyRewards)
            {
                _currenciesManager.AddCurrency(reward.RewardType, reward.Amount);
            }

            _playerProgressionManager.AddExperience(quest.ExperienceReward);
        }

        private void ShowQuestCompletePopup(QuestInfo quest)
        {
            _uiController.ShowMessageBox(_localizationManager.GetLocalizedString("QuestComplete"),
                _localizationManager.GetLocalizedString("QuestCompleteDesc", _localizationManager.GetLocalizedString(quest.Title)));
        }

        private bool IsQuestCompleted(QuestInfo quest)
        {
            var questIdx = _questsState.QuestsStates.FindIndex(qs => qs.QuestID == quest.ID);

            if (questIdx >= 0)
            {
                var questState = _questsState.QuestsStates[questIdx];

                for (var i = 0; i < questState.QuestProgress.Count; i++)
                {
                    if (questState.QuestProgress[i] < quest.QuestTasks[i].NeededAmount)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Awake()
        {
            LoadState();
        }

        protected override void Start()
        {
            base.Start();
            questsCollection.InitialQuests.ForEach(q => StartQuest(q.ID));
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(QUESTS_STATE_FILE_NAME, out _questsState))
            {
                _questsState = new QuestsState
                {
                    QuestsStates = new List<QuestState>(), CurrentQuestIds = new List<string>()
                };
            }
        }

        private void SaveState()
        {
            DataSaver.SavePrefsSecurely(QUESTS_STATE_FILE_NAME, _questsState);
        }
    }

    [Serializable]
    public struct QuestsState
    {
        public List<QuestState> QuestsStates;
        public List<string> CurrentQuestIds;
    }

    [Serializable]
    public struct QuestState
    {
        public string QuestID;
        public List<int> QuestProgress;
    }
}
