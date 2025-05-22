using UnityEngine;
using UnityEngine.Serialization;

namespace HeroicEngine.Components
{
    public class Ragdoll : MonoBehaviour
    {
        [FormerlySerializedAs("animator")]
        [SerializeField] private Animator _animator;
        [FormerlySerializedAs("hips")]
        [SerializeField] private Rigidbody _hips;
        [FormerlySerializedAs("ragdollModeAtStart")]
        [SerializeField] private bool _ragdollModeAtStart = false;

        private void Start()
        {
            SetRagdollMode(_ragdollModeAtStart);
        }

        public void SetHipsAndAnimator(Rigidbody hips, Animator animator)
        {
            _hips = hips;
            _animator = animator;
        }

        /// <summary>
        /// This method sets ragdoll mode. If enabled, character Animator will be disabled and skeleton will become completely physical.
        /// Otherwise, Animator will be active and bones will be moved by animations.
        /// </summary>
        /// <param name="enable">Enable ragdoll?</param>
        public void SetRagdollMode(bool enable)
        {
            if (_animator != null)
            {
                _animator.enabled = !enable;
            }

            var rbs = _hips.transform.GetComponentsInChildren<Rigidbody>();

            foreach (var rb in rbs)
            {
                rb.isKinematic = !enable;

                if (rb.TryGetComponent<Collider>(out var col))
                {
                    col.isTrigger = !enable;
                }
            }
        }

        /// <summary>
        /// Hehe, what can be more funny than pushing ragdoll?
        /// This method applies certain force to the character body with certain direction and automatically activates ragdoll mode on it.
        /// </summary>
        /// <param name="direction">Direction of pushing</param>
        /// <param name="force">Pushing force</param>
        /// <param name="forceMode">Force mode</param>
        public void Push(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
        {
            SetRagdollMode(true);

            _hips.AddForce(direction * force, forceMode);
        }

        /// <summary>
        /// This method applies certain rotation force to the character body and automatically activates its ragdoll mode.
        /// </summary>
        /// <param name="direction">Rotation direction</param>
        /// <param name="torque">Rotation force</param>
        /// <param name="forceMode">Force mode</param>
        public void AddTorque(Vector3 direction, float torque, ForceMode forceMode = ForceMode.Impulse)
        {
            SetRagdollMode(true);

            _hips.AddTorque(direction * torque, forceMode);
        }
    }
}
