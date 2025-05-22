using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Inputs
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        private readonly Dictionary<KeyCode, UnityAction> _keyDownCallbacks = new();
        private readonly Dictionary<KeyCode, UnityAction> _keyUpCallbacks = new();
        
        private Vector3 movementDirection;

        public Vector3 GetMovementDirection()
        {
            return movementDirection;
        }

        public void AddKeyListener(KeyCode key, UnityAction keyDownCallback, UnityAction keyUpCallback)
        {
            if (!_keyDownCallbacks.TryAdd(key, keyDownCallback))
            {
                _keyDownCallbacks[key] += keyDownCallback;
            }

            if (!_keyUpCallbacks.TryAdd(key, keyUpCallback))
            {
                _keyUpCallbacks[key] += keyUpCallback;
            }
        }

        public void RemoveKeyListener(KeyCode key, UnityAction keyDownCallback, UnityAction keyUpCallback)
        {
            if (_keyDownCallbacks.ContainsKey(key))
            {
                _keyDownCallbacks[key] -= keyDownCallback;
            }

            if (_keyUpCallbacks.ContainsKey(key))
            {
                _keyUpCallbacks[key] -= keyUpCallback;
            }
        }

        private void Update()
        {
            movementDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                movementDirection += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                movementDirection -= Vector3.forward;
            }
            if (Input.GetKey(KeyCode.D))
            {
                movementDirection += Vector3.right;
            }
            if (Input.GetKey(KeyCode.A))
            {
                movementDirection -= Vector3.right;
            }

            movementDirection.Normalize();

            foreach (var key in _keyDownCallbacks.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    _keyDownCallbacks[key]?.Invoke();
                }
            }

            foreach (var key in _keyUpCallbacks.Keys)
            {
                if (Input.GetKeyUp(key))
                {
                    _keyUpCallbacks[key]?.Invoke();
                }
            }
        }
    }
}
