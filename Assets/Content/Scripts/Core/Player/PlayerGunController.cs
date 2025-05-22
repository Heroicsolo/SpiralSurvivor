using System.Collections;
using HeroicEngine.Systems.DI;
using HeroicEngine.Utils.Pooling;
using Heroicsolo.SpiralSurvivor.Core.Weapons;
using UnityEngine;

namespace Heroicsolo.SpiralSurvivor.Core.Player
{
    internal sealed class PlayerGunController : SystemBase, IPlayerGunController
    {
        [SerializeField] private Transform _muzzleTransform;
        [SerializeField] private RaymarchingBullet _bulletPrefab;
        [SerializeField] [Min(0f)] private float _shootInterval = 0.3f;

        private bool _isShooting;
        private WaitForSeconds _shootIntervalWaiter;
        private Coroutine _shootingCoroutine;

        public void StartShooting()
        {
            _isShooting = true;
            if (_shootingCoroutine != null)
            {
                StopCoroutine(_shootingCoroutine);
            }
            _shootingCoroutine = StartCoroutine(ShootSequence());
        }
        
        public void EndShooting()
        {
            _isShooting = false;
        }
        
        protected override void Start()
        {
            base.Start();
            _shootIntervalWaiter = new WaitForSeconds(_shootInterval);
            StartShooting();
        }

        private void Shoot()
        {
            var bullet = PoolSystem.GetInstanceAtPosition(_bulletPrefab, _bulletPrefab.GetName(), _muzzleTransform.position, _muzzleTransform.rotation);
            bullet.Launch(_muzzleTransform.forward);
        }

        private IEnumerator ShootSequence()
        {
            do
            {
                Shoot();
                yield return _shootIntervalWaiter;
            } while (_isShooting);
        }
    }
}