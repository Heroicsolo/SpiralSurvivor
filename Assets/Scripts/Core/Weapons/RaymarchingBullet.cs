using HeroicEngine.Utils.Pooling;
using Heroicsolo.SpiralSurvivor.Utils;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.Core.Weapons
{
    public class RaymarchingBullet : MonoBehaviour, IPooledObject
    {
        private const int SDF_STEPS = 20;

        [SerializeField] [Min(0f)] private float _velocity = 60f;
        [SerializeField] [Min(0f)] private float _stepSize = 0.05f;
        [SerializeField] [Min(0f)] private float _maxStepDist = 0.5f;
        [SerializeField] [Min(0f)] private float _maxFlyDistance = 10f;
        [SerializeField] private PooledParticleSystem _collisionParticles;

        private Transform _transform;
        private Vector3 _direction;
        private float _timeToDie;
        private TrailRenderer _trailRenderer;

        public string GetName()
        {
            return gameObject.name;
        }
        public void SetName(string name)
        {
            gameObject.name = name;
        }
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void Launch(Vector3 direction)
        {
            _direction = direction;
            _timeToDie = _maxFlyDistance / _velocity;
            _trailRenderer ??= GetComponentInChildren<TrailRenderer>(true);
            _trailRenderer.emitting = true;
            _trailRenderer.enabled = true;
        }

        private void Start()
        {
            _transform = transform;
        }

        private void PlayCollisionVFX()
        {
            if (_collisionParticles != null)
            {
                PoolSystem.GetInstanceAtPosition(_collisionParticles, _collisionParticles.GetName(), transform.position);
            }
        }

        private void Update()
        {
            if (_timeToDie > 0f)
            {
                _timeToDie -= Time.deltaTime;

                if (_timeToDie <= 0f)
                {
                    _trailRenderer.Clear();
                    _trailRenderer.emitting = false;
                    _trailRenderer.enabled = false;
                    PoolSystem.ReturnToPool(this);
                    return;
                }
            }

            var pos = _transform.position;
            var remainingDist = _velocity * Time.deltaTime;

            for (var i = 0; i < SDF_STEPS && remainingDist > 0f; i++)
            {
                var distToWall = SpiralSDF3D.GetDistance(pos);

                if (distToWall < _stepSize)
                {
                    // Reflect
                    var normal = SpiralSDF3D.EstimateNormal(pos);
                    normal.y = 0f;
                    normal.Normalize();
                    _direction = Vector3.Reflect(_direction, normal).normalized;
                    _direction.y = 0f;
                    _direction.Normalize();

                    PlayCollisionVFX();

                    // Push slightly outside to prevent re-collision
                    pos += normal * (_stepSize * 1.5f);
                    break; // You can also continue to support multiple reflections per frame
                }

                var conservativeDist = Mathf.Min(distToWall * 0.9f, _maxStepDist, remainingDist);

                pos += _direction * conservativeDist;
                remainingDist -= conservativeDist;
            }

            _transform.position = pos;
        }
    }
}
