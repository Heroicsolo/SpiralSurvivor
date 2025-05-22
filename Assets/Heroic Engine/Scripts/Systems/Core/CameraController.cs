using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public sealed class CameraController : SystemBase, ICameraController
    {
        [SerializeField][Min(0f)] private float cameraFollowSpeed = 8f;
        [SerializeField][Min(0f)] private float cameraZoomSpeed = 8f;
        
        private Camera _camera;
        
        private Transform _cameraTransform;
        private Transform _playerTransform;

        private Vector3 _offset;
        private Vector3 _savedPosition;
        private Quaternion _savedRotation;
        private bool _followPlayer;
        private float _currZoom = 1f;
        private float _targetZoom = 1f;
        private float _initFoV;

        public void ResetState()
        {
            _followPlayer = true;
            _cameraTransform.position = _savedPosition;
            _cameraTransform.rotation = _savedRotation;
            _camera.fieldOfView = _initFoV;
        }
        
        public void SetPlayerTransform(Transform playerTransform)
        {
            _camera = Camera.main;
            _cameraTransform = Camera.main?.transform;
            _playerTransform = playerTransform;
            if (_cameraTransform != null)
            {
                _offset = _cameraTransform.position - _playerTransform.position;
            }
            _followPlayer = true;
        }

        public Vector3 GetWorldDirection(Vector3 viewportDirection)
        {
            return _cameraTransform.TransformDirection(viewportDirection);
        }

        public Vector3 GetWorldPosition(Vector3 viewportPosition)
        {
            return _camera.ScreenToWorldPoint(viewportPosition);
        }

        public Ray ScreenPointToRay(Vector3 viewportPosition)
        {
            return _camera.ScreenPointToRay(viewportPosition);
        }

        public void SetZoom(float zoom)
        {
            _targetZoom = zoom;
        }

        public void LookAtFromPos(Vector3 fromPos, Transform targetTransform)
        {
            _camera = Camera.main;
            _cameraTransform = Camera.main?.transform;
            if (_cameraTransform == null)
            {
                return;
            }
            _followPlayer = false;
            _savedPosition = _cameraTransform.position;
            _savedRotation = _cameraTransform.rotation;
            _cameraTransform.position = fromPos;
            _cameraTransform.LookAt(targetTransform.position);
        }

        protected override void Start()
        {
            base.Start();
            if (Camera.main != null)
            {
                _camera = Camera.main;
                _cameraTransform = Camera.main.transform;
                _savedPosition = _cameraTransform.position;
                _savedRotation = _cameraTransform.rotation;
                _initFoV = _camera.fieldOfView;
            }
        }

        private void Update()
        {
            if (_followPlayer && _playerTransform)
            {
                _cameraTransform.position = Vector3.Lerp(_cameraTransform.position,
                    _playerTransform.position + _offset,
                    cameraFollowSpeed * Time.deltaTime);
                _savedPosition = _cameraTransform.position;
                _savedRotation = _cameraTransform.rotation;
            }

            if (_camera != null)
            {
                _currZoom = Mathf.Lerp(_currZoom, _targetZoom, cameraZoomSpeed);
                _camera.fieldOfView = _initFoV / _currZoom;
            }
        }
    }
}