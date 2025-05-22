using HeroicEngine.Systems.DI;
using System.Collections;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Events
{
    public sealed class EventsManager : SystemBase, IEventsManager
    {
        private readonly Hashtable _eventHash = new();

        // Register a listener for an event
        public void RegisterListener<T>(string eventType, UnityAction<T> listener)
        {
            UnityEvent<T> thisEvent;

            var eventKey = GetEventKey<T>(eventType);

            if (_eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T>)_eventHash[eventKey];
                thisEvent.AddListener(listener);
                _eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent<T>();
                thisEvent.AddListener(listener);
                _eventHash.Add(eventKey, thisEvent);
            }
        }

        public void RegisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener)
        {
            UnityEvent<T1, T2> thisEvent;

            var eventKey = GetEventKey<T1, T2>(eventType);

            if (_eventHash.ContainsKey(eventKey))
            {
                thisEvent = (UnityEvent<T1, T2>)_eventHash[eventKey];
                thisEvent.AddListener(listener);
                _eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent<T1, T2>();
                thisEvent.AddListener(listener);
                _eventHash.Add(eventKey, thisEvent);
            }
        }

        public void RegisterListener(string eventType, UnityAction listener)
        {
            UnityEvent thisEvent;

            if (_eventHash.ContainsKey(eventType))
            {
                thisEvent = (UnityEvent)_eventHash[eventType];
                thisEvent.AddListener(listener);
                _eventHash[eventType] = thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                _eventHash.Add(eventType, thisEvent);
            }
        }

        // Unregister a listener for an event
        public void UnregisterListener<T>(string eventType, UnityAction<T> listener)
        {
            var eventKey = GetEventKey<T>(eventType);
            if (_eventHash.ContainsKey(eventKey))
            {
                var thisEvent = (UnityEvent<T>)_eventHash[eventKey];
                thisEvent.RemoveListener(listener);
                _eventHash[eventType] = thisEvent;
            }
        }

        public void UnregisterListener<T1, T2>(string eventType, UnityAction<T1, T2> listener)
        {
            var eventKey = GetEventKey<T1, T2>(eventType);
            if (_eventHash.ContainsKey(eventKey))
            {
                var thisEvent = (UnityEvent<T1, T2>)_eventHash[eventKey];
                thisEvent.RemoveListener(listener);
                _eventHash[eventType] = thisEvent;
            }
        }

        public void UnregisterListener(string eventType, UnityAction listener)
        {

            if (_eventHash.ContainsKey(eventType))
            {
                var thisEvent = (UnityEvent)_eventHash[eventType];
                thisEvent.RemoveListener(listener);
                _eventHash[eventType] = thisEvent;
            }
        }

        // Trigger an event (calls all listeners associated with this event)
        public void TriggerEvent<T>(string eventType, T value)
        {
            var eventKey = GetEventKey<T>(eventType);
            if (_eventHash.ContainsKey(eventKey))
            {
                var thisEvent = (UnityEvent<T>)_eventHash[eventKey];
                thisEvent.Invoke(value);
            }
        }

        public void TriggerEvent<T1, T2>(string eventType, T1 value1, T2 value2)
        {
            var eventKey = GetEventKey<T1, T2>(eventType);
            if (_eventHash.ContainsKey(eventKey))
            {
                var thisEvent = (UnityEvent<T1, T2>)_eventHash[eventKey];
                thisEvent.Invoke(value1, value2);
            }
        }

        public void TriggerEvent(string eventType)
        {
            if (_eventHash.ContainsKey(eventType))
            {
                var thisEvent = (UnityEvent)_eventHash[eventType];
                thisEvent.Invoke();
            }
        }

        private static string GetEventKey<T>(string eventName)
        {
            var type = typeof(T);
            var key = type + eventName;
            return key;
        }

        private static string GetEventKey<T1, T2>(string eventName)
        {
            var type1 = typeof(T1);
            var type2 = typeof(T2);
            var key = type1 + type2.ToString() + eventName;
            return key;
        }
    }
}
