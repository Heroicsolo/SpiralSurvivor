using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Components
{
    public class FloatingItem : MonoBehaviour
    {
        [FormerlySerializedAs("m_floatingAmplitude")]
        [SerializeField] private float _floatingAmplitude = 1f;
        [FormerlySerializedAs("m_floatingSpeed")]
        [SerializeField] private float _floatingSpeed = 2f;

        private Vector3 initPos;

        private void Start()
        {
            initPos = transform.localPosition;
        }

        /// <summary>
        /// This method instantly moves gameobject to initial position.
        /// </summary>
        public void Restart()
        {
            transform.localPosition = initPos;
        }

        private void FixedUpdate()
        {
            transform.localPosition = initPos + transform.up * (_floatingAmplitude * Mathf.Sin(_floatingSpeed * Time.fixedTime));
        }
    }
}