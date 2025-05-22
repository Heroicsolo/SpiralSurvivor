using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Utils;
using HeroicEngine.Utils.Editor;
using HeroicEngine.Utils.Math;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Components.Combat
{
    public class Projectile : PooledObject
    {
        [Header("Movement")]
        [SerializeField] [Min(0f)] private float velocity = 10f;
        [SerializeField] private bool guiding = false;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Range(0f, 1f)] private float guidingForce = 1f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Range(0f, 180f)] private float guidingAngle = 30f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] private float guidingMaxDist = 200f;
        [ConditionalHide("guiding", true, true)]
        [SerializeField] [Min(0f)] private float guidingAngularSpeed = 4f;
        [SerializeField] [Min(0f)] private float lifetime = 3f;

        [Header("Damage")]
        [SerializeField] [Min(0f)] private float damageMin = 0f;
        [SerializeField] [Min(0f)] private float damageMax = 0f;
        [SerializeField] [Min(0f)] private float explosionRadius = 1f;

        [Header("Effects")]
        [SerializeField] private PooledParticleSystem explosionEffect;
        [SerializeField] private AudioClip launchSound;
        [SerializeField] private AudioClip hitSound;

        [Inject] private IHittablesManager _hittablesManager;
        [Inject] private ISoundsManager _soundsManager;

        private Transform _targetTransform;
        private TeamType _teamType;
        private bool _isLaunched;
        private float _timeToDie;
        private Quaternion _targetRot;

        /// <summary>
        /// This method launches projectile from certain owner to the certain target.
        /// If owner is not set, this projectile will be neutral (TeamType.None) and will be able to inflict damage both to player team and enemies team.
        /// </summary>
        /// <param name="target">Target transform</param>
        /// <param name="owner">Owner of this projectile</param>
        public void Launch(Transform target, IHittable owner = null)
        {
            InjectionManager.InjectTo(this);

            _targetTransform = target;
            _teamType = owner?.TeamType ?? TeamType.None;
            _timeToDie = lifetime;
            _isLaunched = true;
            transform.LookAt(target);

            if (launchSound != null)
            {
                _soundsManager.PlayClip(launchSound);
            }
        }

        /// <summary>
        /// This method launches projectile from certain owner to the certain world direction.
        /// If owner is not set, this projectile will be neutral (TeamType.None) and will be able to inflict damage both to player team and enemies team.
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="owner">Owner of this projectile</param>
        public void Launch(Vector3 direction, IHittable owner = null)
        {
            InjectionManager.InjectTo(this);

            _targetTransform = null;
            _teamType = owner?.TeamType ?? TeamType.None;
            _timeToDie = lifetime;
            _isLaunched = true;
            transform.rotation = Quaternion.LookRotation(direction);

            if (launchSound != null)
            {
                _soundsManager.PlayClip(launchSound);
            }
        }

        private void Explode()
        {
            if (explosionEffect != null)
            {
                PoolSystem.GetInstanceAtPosition(explosionEffect, explosionEffect.name, transform.position);
            }

            var hittables = _hittablesManager.GetOtherTeamsHittablesInRadius(transform.position, explosionRadius, _teamType);

            if (hittables.Count > 0)
            {
                hittables.ForEach(hittable => hittable.GetDamage(Random.Range(damageMin, damageMax)));
            }

            _targetTransform = null;
            _isLaunched = false;

            PoolSystem.ReturnToPool(this);
        }

        private void Update()
        {
            if (!_isLaunched)
            {
                return;
            }

            if (_timeToDie > 0f)
            {
                _timeToDie -= Time.deltaTime;

                if (_timeToDie <= 0f)
                {
                    Explode();
                    return;
                }
            }

            if (guiding && guidingForce > 0f && _targetTransform == null && _targetTransform.Distance(transform) < guidingMaxDist)
            {
                var dir = (_targetTransform.position - transform.position).normalized;

                var localDir = transform.InverseTransformDirection(dir);

                if (localDir.z > 0f && Mathf.Atan2(localDir.z, localDir.x) < guidingAngle)
                {
                    var guidedDir = dir * guidingForce + transform.forward * (1f - guidingForce);
                    _targetRot = Quaternion.LookRotation(guidedDir, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, _targetRot, guidingAngularSpeed * Time.deltaTime);
                }
                else
                {
                    _targetTransform = null;
                }
            }
            else
            {
                _targetTransform = null;
            }

            transform.Translate(Time.deltaTime * velocity * transform.forward, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.GetComponentInParent<Hittable>();

            if (hittable != null && hittable.TeamType != _teamType)
            {
                if (!hittable.IsDead())
                {
                    hittable.GetDamage(Random.Range(damageMin, damageMax));
                }

                if (hitSound != null)
                {
                    _soundsManager.PlayClip(hitSound);
                }

                Explode();
            }
            else if (hittable == null && !other.isTrigger)
            {
                if (hitSound != null)
                {
                    _soundsManager.PlayClip(hitSound);
                }

                Explode();
            }
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);
        }
    }
}
