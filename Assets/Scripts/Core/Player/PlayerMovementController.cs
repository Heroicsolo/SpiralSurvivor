using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Inputs;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.Core.Player
{
    internal sealed class PlayerMovementController : MonoBehaviour
    {
        [SerializeField][Min(0f)] private float _moveSpeed = 5f;

        [Inject] private ICameraController _cameraController;
        [Inject] private IInputManager _inputManager;

        private CharacterController _characterController;
        private float _initY;
        private Transform _transform;

        private void Start()
        {
            InjectionManager.InjectTo(this);

            _cameraController.SetPlayerTransform(transform);

            _characterController = GetComponent<CharacterController>();
            _transform = transform;
            _initY = _transform.position.y;
        }

        private void Update()
        {
            var movementVector = _cameraController.GetWorldDirection(_inputManager.GetMovementDirection());
            movementVector.y = 0f;
            movementVector.Normalize();
            _characterController.Move(_moveSpeed * Time.deltaTime * movementVector);
        }

        private void LateUpdate()
        {
            var fixedPos = _transform.position;
            fixedPos.y = _initY;
            _transform.position = fixedPos;
        }
    }
}