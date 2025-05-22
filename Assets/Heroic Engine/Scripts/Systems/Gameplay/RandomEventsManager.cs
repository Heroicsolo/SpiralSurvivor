using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Utils.Data;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public sealed class RandomEventsManager : SystemBase, IRandomEventsManager
    {
        private const string STATE_FILE_NAME = "RandomEventsState";

        [SerializeField] private RandomEventsCollection possibleEvents;

        [Inject] private ISoundsManager _soundsManager;

        private RandomEventsState _randomEventsState;

        public void RegisterEvent(RandomEventInfo eventInfo)
        {
            possibleEvents.RegisterItem(eventInfo);
        }

        public float GetEventChance(string eventType)
        {
            var eventInfo = possibleEvents.Items.Find(e => e.EventType == eventType);

            var modifiedChance = eventInfo.Chance;

            var stateIdx = _randomEventsState.eventsStates.FindIndex(es => es.eventType == eventType);

            if (stateIdx >= 0)
            {
                modifiedChance += _randomEventsState.eventsStates[stateIdx].dropChanceModifier;
            }

            return modifiedChance;
        }

        public void ResetEventChance(string eventType)
        {
            var stateIdx = _randomEventsState.eventsStates.FindIndex(es => es.eventType == eventType);

            if (stateIdx >= 0)
            {
                var eventState = new RandomEventState
                {
                    eventType = eventType, dropChanceModifier = 0f
                };

                _randomEventsState.eventsStates[stateIdx] = eventState;

                SaveState();
            }
        }

        public bool DoEventAttempt(RandomEventInfo eventInfo)
        {
            if (eventInfo.BadLuckProtection)
            {
                var modifiedChance = eventInfo.Chance;

                var stateIdx = _randomEventsState.eventsStates.FindIndex(es => es.eventType == eventInfo.EventType);

                if (stateIdx >= 0)
                {
                    modifiedChance += _randomEventsState.eventsStates[stateIdx].dropChanceModifier;
                }

                var isSuccess = UnityEngine.Random.value <= modifiedChance;

                var modifiedState = new RandomEventState
                {
                    eventType = eventInfo.EventType, dropChanceModifier = stateIdx >= 0 ? _randomEventsState.eventsStates[stateIdx].dropChanceModifier : 0f
                };

                if (isSuccess)
                {
                    if (eventInfo.EventSound != null)
                    {
                        _soundsManager.PlayClip(eventInfo.EventSound);
                    }

                    if (eventInfo.GoodLuckProtection)
                    {
                        modifiedState.dropChanceModifier = -(modifiedChance - eventInfo.Chance);
                    }
                    else
                    {
                        modifiedState.dropChanceModifier = 0f;
                    }
                }
                else
                {
                    modifiedState.dropChanceModifier += eventInfo.Chance;
                }

                if (stateIdx >= 0)
                {
                    _randomEventsState.eventsStates[stateIdx] = modifiedState;
                }
                else
                {
                    _randomEventsState.eventsStates.Add(modifiedState);
                }

                SaveState();

                return isSuccess;
            }

            return UnityEngine.Random.value <= eventInfo.Chance;
        }

        public bool DoEventAttempt(string eventType)
        {
            var eventInfo = possibleEvents.Items.Find(e => e.EventType == eventType);

            if (eventInfo == null)
            {
                return false;
            }

            return DoEventAttempt(eventInfo);
        }

        private void LoadState()
        {
            if (!DataSaver.LoadPrefsSecurely(STATE_FILE_NAME, out _randomEventsState))
            {
                _randomEventsState.eventsStates = new List<RandomEventState>();
            }
        }

        private void SaveState()
        {
            DataSaver.SavePrefsSecurely(STATE_FILE_NAME, _randomEventsState);
        }

        private void Awake()
        {
            LoadState();
        }

        [Serializable]
        private struct RandomEventsState
        {
            public List<RandomEventState> eventsStates;
        }

        [Serializable]
        private struct RandomEventState
        {
            public string eventType;
            public float dropChanceModifier;
        }
    }
}
