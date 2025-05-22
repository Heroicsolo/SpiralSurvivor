using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.ScenesManagement;
using UnityEngine;

namespace HeroicEngine.Systems
{
    public class TimeManager : SystemBase, ITimeManager
    {
        private float _initialTimeScale;

        [Inject] private ScenesLoader _scenesLoader;

        public void PauseGame()
        {
            if (!_scenesLoader.IsSceneLoading())
            {
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            Time.timeScale = _initialTimeScale;
        }

        public void SetTimeScale(float timeScale)
        {
            _initialTimeScale = timeScale;
            ResumeGame();
        }

        protected override void Start()
        {
            base.Start();
            _initialTimeScale = Time.timeScale;
        }
    }
}
