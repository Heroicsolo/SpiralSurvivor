using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using System;
using UnityEngine;

namespace HeroicEngine.UI
{
    public sealed class InfoPopup : UIPart
    {
        [SerializeField] private bool pauseGame = false;

        [Inject] private ITimeManager _timeManager;

        private Action _hideCallback;

        public void SetHideCallback(Action hideCallback)
        {
            this._hideCallback = hideCallback;
        }

        public override void Show()
        {
            InjectionManager.InjectTo(this);

            base.Show();
            if (pauseGame)
            {
                _timeManager.PauseGame();
            }
        }

        public override void Hide()
        {
            base.Hide();
            _hideCallback?.Invoke();
            if (pauseGame)
            {
                _timeManager.ResumeGame();
            }
        }
    }
}