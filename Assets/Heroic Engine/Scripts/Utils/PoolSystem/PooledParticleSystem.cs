using UnityEngine;

namespace HeroicEngine.Utils.Pooling
{
    public class PooledParticleSystem : PooledObject
    {
        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!_particles.IsAlive())
            {
                PoolSystem.ReturnToPool(this);
            }
        }
    }
}