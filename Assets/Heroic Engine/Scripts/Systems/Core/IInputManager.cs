using HeroicEngine.Systems.DI;
using UnityEngine;
using UnityEngine.Events;

namespace HeroicEngine.Systems.Inputs
{
    public interface IInputManager : ISystem
    {
        Vector3 GetMovementDirection();
        void AddKeyListener(KeyCode key, UnityAction keyDownCallback, UnityAction keyUpCallback);
        void RemoveKeyListener(KeyCode key, UnityAction keyDownCallback, UnityAction keyUpCallback);
    }
}