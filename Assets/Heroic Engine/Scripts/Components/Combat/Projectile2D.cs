using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Utils.Editor;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Components.Combat
{
    public class Projectile2D : PooledObject
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
        private float _targetAngle;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Transform target, IHittable owner = null)
        {
            InjectionManager.InjectTo(this);

            _targetTransform = target;
            _teamType = owner?.TeamType ?? TeamType.None;
            _timeToDie = lifetime;
            _isLaunched = true;

            Vector2 direction = (target.position - transform.position).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (launchSound != null)
            {
                _soundsManager.PlayClip(launchSound);
            }
        }

        public void Launch(Vector3 direction, IHittable owner = null)
        {
            InjectionManager.InjectTo(this);

            _targetTransform = null;
            _teamType = owner?.TeamType ?? TeamType.None;
            _timeToDie = lifetime;
            _isLaunched = true;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

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

                var hittables = _hittablesManager.GetOtherTeamsHittablesInRadius(transform.position, explosionRadius, _teamType);

                if (hittables.Count > 0)
                {
                    hittables.ForEach(hittable => hittable.GetDamage(Random.Range(damageMin, damageMax)));
                }
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

            if (guiding && guidingForce > 0f && _targetTransform != null && Vector2.Distance(_targetTransform.position, transform.position) < guidingMaxDist)
            {
                Vector2 dir = (_targetTransform.position - transform.position).normalized;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, angle)) < guidingAngle)
                {
                    var guidedAngle = Mathf.LerpAngle(transform.eulerAngles.z, angle, guidingForce);
                    transform.rotation = Quaternion.Euler(0, 0, guidedAngle);
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

            _rb.linearVelocity = transform.right * velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
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
