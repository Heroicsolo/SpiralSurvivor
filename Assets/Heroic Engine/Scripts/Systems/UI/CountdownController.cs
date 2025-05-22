using HeroicEngine.Systems.Audio;
using HeroicEngine.Utils;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.UI
{
    public sealed class CountdownController : MonoBehaviour, ICountdownController
    {
        [SerializeField] private List<AudioClip> countdownSounds = new();

        [Inject] private ISoundsManager _soundsManager;
        [Inject] private IUIController _uiController;

        private Action _tickCallback;
        private Action _endCallback;
        private Action _cancelCallback;
        private SlowUpdate _slowUpdate;
        private float _lifetime;

        public void StartCountdown(float seconds, Action tickCallback, Action endCallback, Action cancelCallback = null)
        {
            if (seconds <= 0)
            {
                return;
            }

            _tickCallback = tickCallback;
            _endCallback = endCallback;
            _cancelCallback = cancelCallback;

            if (_slowUpdate != null && _slowUpdate.IsRunning())
            {
                _slowUpdate.Stop();
            }

            _slowUpdate = new SlowUpdate(this, CountdownTick, 1f);
            _slowUpdate.Run();

            _lifetime = seconds;

            CountdownTick();
        }

        public void CancelCountdown()
        {
            _lifetime = 0f;
            _uiController.HideAnnouncement();
            _slowUpdate.Stop();
            _cancelCallback?.Invoke();
        }

        private void CountdownTick()
        {
            var second = Mathf.RoundToInt(_lifetime);
            if (countdownSounds.Count >= second && second > 0)
            {
                _soundsManager.PlayClip(countdownSounds[second - 1]);
            }
            _uiController.ShowAnnouncement(second.ToString());
            _tickCallback?.Invoke();
        }

        private void Update()
        {
            if (_lifetime > 0f)
            {
                _lifetime -= Time.deltaTime;

                if (_lifetime <= 0f)
                {
                    _lifetime = 0f;
                    _slowUpdate.Stop();
                    _uiController.HideAnnouncement();
                    _endCallback?.Invoke();
                }
            }
        }
    }
}