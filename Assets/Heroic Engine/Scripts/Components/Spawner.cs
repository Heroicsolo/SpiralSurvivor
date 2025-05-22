using HeroicEngine.Utils.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Utils.Pooling;
using HeroicEngine.Utils.Math;

namespace HeroicEngine.Components
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject objectPrefab;
        [SerializeField] private List<Transform> spawnPoints = new();
        [SerializeField] private SpawnerLaunchMode launchMode = SpawnerLaunchMode.AtStart;
        [ConditionalHide("launchMode", true, SpawnerLaunchMode.OnTriggerEnter)]
        [SerializeField] private LayerMask triggerLayerMask;
        [SerializeField] [Min(0f)] private float startSpawnDelay = 1f;
        [SerializeField] [Min(0f)] private float spawnPeriod = 1f;
        [SerializeField] [Min(0)] private int spawnCount = 1;

        private Action _spawnCallback;
        private Action _spawnEndCallback;
        private bool _isRunning;
        private int _waveNumber;
        private List<GameObject> _spawnedObjects = new();

        /// <summary>
        /// This method sets a certain action which will be invoked each time when object will be spawned.
        /// </summary>
        /// <param name="spawnCallback">Spawn callback</param>
        public void SetSpawnCallback(Action spawnCallback)
        {
            _spawnCallback = spawnCallback;
        }

        /// <summary>
        /// This method sets a certain action which will be invoked in the end of whole spawning process.
        /// </summary>
        /// <param name="spawnEndCallback">Spawn end callback</param>
        public void SetSpawnEndCallback(Action spawnEndCallback)
        {
            _spawnEndCallback = spawnEndCallback;
        }

        /// <summary>
        /// This method sets spawn object prefab. If this prefab is inherited from PooledObject, it will be pooled via PoolSystem.
        /// </summary>
        /// <param name="prefab">Spawn object prefab</param>
        public void SetSpawnObjectPrefab(GameObject prefab)
        {
            objectPrefab = prefab;
        }

        /// <summary>
        /// This method sets spawn period (in seconds).
        /// </summary>
        /// <param name="period">Spawn period (in seconds)</param>
        public void SetSpawnPeriod(float period)
        {
            spawnPeriod = period;
        }

        /// <summary>
        /// This method destroys all objects spawned by this spawner (if that objects were pooled, they return back to poll).
        /// </summary>
        public void RemoveSpawnedObjects()
        {
            foreach (var obj in _spawnedObjects.ToArray())
            {
                if (obj != null)
                {
                    if (obj.TryGetComponent<PooledObject>(out var pooled))
                    {
                        PoolSystem.ReturnToPool(pooled);
                    }
                    else
                    {
                        Destroy(obj);
                    }
                }
            }

            _spawnedObjects.Clear();
        }

        /// <summary>
        /// This method launches spawning process.
        /// </summary>
        public void Launch()
        {
            if (!_isRunning)
            {
                _waveNumber = 0;
                StartCoroutine(SpawnCoroutine());
            }
        }

        /// <summary>
        /// This method stops spawning process.
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                StopAllCoroutines();
            }
        }

        private void Start()
        {
            if (!_isRunning && launchMode == SpawnerLaunchMode.AtStart)
            {
                Launch();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isRunning && (triggerLayerMask.value & 1 << other.gameObject.layer) != 0)
            {
                Launch();
            }
        }

        private IEnumerator SpawnCoroutine()
        {
            _isRunning = true;

            yield return new WaitForSeconds(startSpawnDelay);

            while (_waveNumber < spawnCount && objectPrefab != null)
            {
                GameObject spawnedObj = null;

                var pos = spawnPoints.Count > 0 ? spawnPoints.GetRandomElement().position : transform.position;

                if (objectPrefab.TryGetComponent<PooledObject>(out var pooledObj))
                {
                    spawnedObj = PoolSystem.GetInstanceAtPosition(pooledObj, pooledObj.GetName(), pos).GetGameObject();
                }
                else
                {
                    spawnedObj = Instantiate(objectPrefab, pos, Quaternion.identity);
                }

                _spawnedObjects.Add(spawnedObj);

                _spawnCallback?.Invoke();

                _waveNumber++;

                yield return new WaitForSeconds(spawnPeriod);
            }

            _spawnEndCallback?.Invoke();
        }
    }

    [Serializable]
    public enum SpawnerLaunchMode
    {
        None = 0,
        AtStart = 1,
        OnTriggerEnter = 2
    }
}
