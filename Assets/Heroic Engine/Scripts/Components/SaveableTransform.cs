using System;
using UnityEngine;

namespace HeroicEngine.Components
{
    public class SaveableTransform : MonoBehaviour, ISaveableTransform
    {
        [SerializeField] private ObjectSaveMode saveMode;

        private void Awake()
        {
            LoadData();
        }

        private void OnDisable()
        {
            if ((saveMode & ObjectSaveMode.OnDisable) != 0)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            if ((saveMode & ObjectSaveMode.OnApplicationExit) != 0)
            {
                Save();
            }
        }

        public void Save()
        {
            SaveToPlayerPrefs();
        }

        private void SaveToPlayerPrefs()
        {
            var key = gameObject.name + "_" + GetInstanceID();
            var json = JsonUtility.ToJson(new TransformData(transform));
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            var key = gameObject.name + "_" + GetInstanceID();

            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                var tdata = JsonUtility.FromJson<TransformData>(json);
                tdata.ApplyToTransform(transform);
            }
        }
    }

    [Flags]
    public enum ObjectSaveMode
    {
        OnApplicationExit = 1 << 0,
        OnDisable = 1 << 1,
        OnDemand = 1 << 2
    }

    [Serializable]
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }

        public void ApplyToTransform(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }
    }
}
