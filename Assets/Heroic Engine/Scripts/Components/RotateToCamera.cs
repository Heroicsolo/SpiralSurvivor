using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Components
{
    /* 
     * - Just add this component to your game object and it will automatically rotate to main camera.
     * - You can also assign an anchor transform and set needed anchor distance via appropriate parameters. It can help to place some labels near your game characters, for example. Otherwise, it will use its own transform as anchor.
     */

    public class RotateToCamera : MonoBehaviour
    {
        [FormerlySerializedAs("m_anchorTransform")]
        [SerializeField] private Transform _anchorTransform;
        [FormerlySerializedAs("m_anchorDist")]
        [SerializeField] private float _anchorDist = 0f;
        
        private Transform _transform;
        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        
        private void Start()
        {
            _transform = transform;
            _mainCamera = Camera.main;
            _mainCameraTransform = _mainCamera?.transform;
        }
        
        private void LateUpdate()
        {
            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
                _mainCameraTransform = _mainCamera?.transform;
            }

            if (_anchorTransform && _mainCameraTransform)
            {
                _transform.position = _anchorTransform.position - _mainCameraTransform.rotation * (_anchorDist * Vector3.forward);
            }

            if (_mainCameraTransform)
            {
                _transform.LookAt(
                    _transform.position + _mainCameraTransform.rotation * Vector3.forward,
                    _mainCameraTransform.rotation * Vector3.up);
            }
        }
    }
}