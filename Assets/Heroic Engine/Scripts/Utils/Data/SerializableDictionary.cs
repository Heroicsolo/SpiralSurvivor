using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    [Serializable]
    public class SerializableDictionary<TKey>
    {
        [SerializeField]
        private List<KeyValuePair> items = new();

        [Serializable]
        public class KeyValuePair
        {
            public TKey Key;
            public string Value; // Store as string to handle any object type

            public KeyValuePair() { } // Parameterless constructor for serialization
            public KeyValuePair(TKey key, string value)
            {
                Key = key;
                Value = value;
            }
        }

        public void Add(TKey key, object value)
        {
            items.Add(new KeyValuePair(key, JsonUtility.ToJson(value))); // Serialize the object to JSON string
        }

        public bool TryGetValue(TKey key, out object value)
        {
            foreach (var item in items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, key))
                {
                    value = JsonUtility.FromJson<object>(item.Value); // Deserialize the JSON string
                    return true;
                }
            }
            value = null;
            return false;
        }

        public Dictionary<TKey, object> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, object>();
            foreach (var item in items)
            {
                dictionary[item.Key] = JsonUtility.FromJson<object>(item.Value); // Deserialize the JSON string
            }
            return dictionary;
        }

        public SerializableDictionary(Dictionary<TKey, object> dictionary)
        {
            items.Clear();
            foreach (var kvp in dictionary)
            {
                items.Add(new KeyValuePair(kvp.Key, JsonUtility.ToJson(kvp.Value))); // Serialize the object to JSON string
            }
        }

        public List<KeyValuePair> Items => items; // Expose items for serialization
    }
}