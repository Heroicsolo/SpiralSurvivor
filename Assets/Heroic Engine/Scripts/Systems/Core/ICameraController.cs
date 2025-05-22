using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public interface ICameraController : ISystem
    {
        void SetPlayerTransform(Transform playerTransform);
        Vector3 GetWorldDirection(Vector3 viewportDirection);
        Vector3 GetWorldPosition(Vector3 viewportPosition);
        Ray ScreenPointToRay(Vector3 viewportPosition);
        void LookAtFromPos(Vector3 fromPos, Transform targetTransform);
        void SetZoom(float zoom);
        void ResetState();
    }
}