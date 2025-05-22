using System.Collections;
using UnityEngine;

namespace HeroicEngine.Components
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationDirection;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private bool localSpace = true;
        
        private Transform _transform;
        private Coroutine rotationCoroutine;

        private void OnEnable()
        {
            StopRotation();
            _transform = GetComponent<Transform>();
            rotationCoroutine = StartCoroutine(Rotate());
        }

        private void OnDisable()
        {
            StopRotation();
        }

        private IEnumerator Rotate()
        {
            do
            {
                _transform.Rotate(rotationDirection * (rotationSpeed * Time.deltaTime), localSpace ? Space.Self : Space.World);
                yield return null;
            } while (true);
        }

        private void StopRotation()
        {
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
            }
        }
    }
}