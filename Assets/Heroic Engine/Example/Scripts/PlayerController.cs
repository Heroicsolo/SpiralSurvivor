using HeroicEngine.Systems;
using HeroicEngine.Systems.Inputs;
using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Examples
{
    internal sealed class PlayerController : MonoBehaviour
    {
        [SerializeField][Min(0f)] private float moveSpeed = 5f;

        [Inject] private ICameraController _cameraController;
        [Inject] private IInputManager _inputManager;

        private CharacterController _characterController;

        private void Start()
        {
            InjectionManager.InjectTo(this);

            _cameraController.SetPlayerTransform(transform);

            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var movementVector = _cameraController.GetWorldDirection(_inputManager.GetMovementDirection());
            movementVector.y = 0f;
            _characterController.Move(moveSpeed * Time.deltaTime * movementVector);
        }
    }
}