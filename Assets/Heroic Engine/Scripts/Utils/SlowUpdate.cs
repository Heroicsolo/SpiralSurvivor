using System;
using System.Collections;
using UnityEngine;

namespace HeroicEngine.Utils
{
    public class SlowUpdate
    {
        private readonly MonoBehaviour _owner;
        private readonly Action _action;
        private readonly float _period;

        private Coroutine _coroutine;
        private bool _isRunning;

        public SlowUpdate(MonoBehaviour owner, Action action, float period)
        {
            _owner = owner;
            _action = action;
            _period = period;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public void Run()
        {
            if (_action != null && _owner != null && _owner.isActiveAndEnabled)
            {
                _coroutine = _owner.StartCoroutine(Updater());
                _isRunning = true;
            }
        }

        public void Stop()
        {
            if (_coroutine != null && _owner)
            {
                _owner.StopCoroutine(_coroutine);
                _isRunning = false;
            }
        }

        private IEnumerator Updater()
        {
            while (_owner && _owner.isActiveAndEnabled)
            {
                yield return new WaitForSeconds(_period);
                _action.Invoke();
            }
        }
    }
}