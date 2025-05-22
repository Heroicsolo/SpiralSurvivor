using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Utils.HidingObjects
{
    //This is hiding objects manager - script that checks if some AutoHidingObject is situated in front of camera
    //Add AutoHidingObject script to all objects which you need to hide (that objects have to contain MeshRenderer component)
    //Add this component to your camera and set needed objects check distance
    public sealed class HidingObjectsManager : MonoBehaviour
    {
        private const float TICK_PERIOD = 0.5f;
        private const int MAX_COLLIDERS_TO_FIND = 50;
        private const int SEARCH_RADIUS = 8;

        [FormerlySerializedAs("m_distance")]
        [SerializeField]
        private float _distance = 25f;
        [SerializeField] private Camera cam;

        private readonly List<AutoHidingObject> _mAhoList = new();
        private readonly List<AutoHidingObject> _hiddenObjects = new();

        private Transform _mPlayerTransform;
        private float _tickDelay = TICK_PERIOD;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[5];

        public void SetPlayerTransform(Transform playerTransform)
        {
            _mPlayerTransform = playerTransform;
        }

        private void Start()
        {
            _mAhoList.Clear();
        }

        private bool InFrontOfPlayer(Transform t)
        {
            if (Physics.RaycastNonAlloc(transform.position, _mPlayerTransform.position - transform.position, _raycastHits, _distance) > 0)
            {
                for (var i = 0; i < _raycastHits.Length; i++)
                {
                    if (_raycastHits[i].transform == t)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool InScreenArea(Transform t)
        {
            var pos = cam.WorldToViewportPoint(t.position);
            return pos.x > 0 && pos.x < 1 &&
                pos.y > 0 && pos.y < 1 && pos.z > 0;
        }

        private void Update()
        {
            if (_tickDelay > 0)
            {
                _tickDelay -= Time.deltaTime;
                return;
            }

            _tickDelay = TICK_PERIOD;

            ProcessPreviouslyHidden();

            RefreshNearObjects();

            foreach (var aho in _mAhoList)
            {
                if (aho == null) continue;

                if (InScreenArea(aho.transform) && InFrontOfPlayer(aho.transform))
                {
                    aho.Hide();
                    _hiddenObjects.Add(aho);
                }
                else
                {
                    aho.Show();
                }
            }
        }

        private void ProcessPreviouslyHidden()
        {
            for (var i = _hiddenObjects.Count - 1; i >= 0; i--)
            {
                if (_hiddenObjects[i] == null)
                {
                    _hiddenObjects.Remove(_hiddenObjects[i]);
                    continue;
                }

                if (InFrontOfPlayer(_hiddenObjects[i].transform) == false ||
                    InScreenArea(_hiddenObjects[i].transform) == false)
                {
                    _hiddenObjects[i].Show();
                    _hiddenObjects.Remove(_hiddenObjects[i]);
                }
            }
        }

        private void RefreshNearObjects()
        {
            _mAhoList.Clear();

            var maxColliders = MAX_COLLIDERS_TO_FIND;
            var hitColliders = new Collider[maxColliders];
            var numColliders = Physics.OverlapSphereNonAlloc(_mPlayerTransform.position, SEARCH_RADIUS, hitColliders);
            for (var i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].TryGetComponent(out AutoHidingObject objetToHide))
                {
                    _mAhoList.Add(objetToHide);
                }
            }
        }
    }
}
