using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Components
{
    //This component moves material texture which can be useful for conveyor effect or some fake "monitor" animations
    //All you need is to add this component to your GameObject with MeshRenderer component and set movementVector value in inspector
    public class TextureMover : MonoBehaviour
    {
        [FormerlySerializedAs("m_movementVector")]
        [SerializeField] private Vector2 _movementVector;
        [FormerlySerializedAs("m_sinMovement")]
        [SerializeField] private bool _sinMovement = false;
        [FormerlySerializedAs("m_movementSpeed")]
        [SerializeField] private float _movementSpeed = 1f;

        private Material _mat;
        
        private void Awake()
        {
            _mat = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            if (!_sinMovement)
            {
                _mat.mainTextureOffset += _movementVector * Time.deltaTime;
            }
            else
            {
                _mat.mainTextureOffset = _movementVector * Mathf.Sin(_movementSpeed * Time.time);
            }
        }
    }
}