using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Inputs;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.Core.Player
{
    internal sealed class PlayerAimController : MonoBehaviour
    {
        [SerializeField] [Min(0f)] private float _rotationSpeed = 10f;
        [SerializeField] [Min(0f)] private float _aimZoom = 0.9f;

        [Inject] private IInputManager _inputManager;
        [Inject] private ICameraController _cameraController;

        private Transform _transform;
        private bool _isAiming;
        private Plane _aimPlane;

        private void Start()
        {
            InjectionManager.InjectTo(this);

            _transform = transform;
            _aimPlane = new Plane(Vector3.up, Vector3.zero);

            _inputManager.AddKeyListener(KeyCode.Mouse0, OnMouseDown, OnMouseUp);
        }

        private void OnMouseDown()
        {
            _isAiming = true;
            _cameraController.SetZoom(_aimZoom);
        }

        private void OnMouseUp()
        {
            _isAiming = false;
            _cameraController.SetZoom(1f);
        }

        private void Aim()
        {
            var ray = _cameraController.ScreenPointToRay(Input.mousePosition);

            if (_aimPlane.Raycast(ray, out var distance))
            {
                var point = ray.GetPoint(distance);
                var lookDir = point - _transform.position;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.01f)
                {
                    var targetRot = Quaternion.LookRotation(lookDir);
                    _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRot, Time.deltaTime * _rotationSpeed);
                }
            }
        }

        private void Update()
        {
            if (_isAiming)
            {
                Aim();
            }
        }
    }
}
