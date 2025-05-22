using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.UI;
using HeroicEngine.Systems.DI;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.UI
{
    public class UIPart : MonoBehaviour
    {
        [SerializeField] private UIPartType partType;
        [SerializeField] private AudioClip showSound;

        [Inject] private IUIController _uiController;
        [Inject] private ISoundsManager _soundsManager;

        protected float _timeToHide;

        public UIPartType PartType => partType;

        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (showSound)
            {
                InjectionManager.InjectTo(this);
                _soundsManager.PlayClip(showSound);
            }
        }

        public void ShowTemporary(float timeLength)
        {
            Show();
            _timeToHide = timeLength;
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            InjectionManager.InjectTo(this);

            // If this UI Part is child object of UIController object, UIController won't be injected,
            // so we need to find it on scene
            _uiController ??= FindObjectOfType<UIController>();
            _uiController.RegisterUIPart(this);

            _soundsManager ??= FindObjectOfType<SoundsManager>();
        }

        private void OnDestroy()
        {
            _uiController.UnregisterUIPart(this);
        }

        private void Update()
        {
            if (_timeToHide > 0f)
            {
                _timeToHide -= Time.deltaTime;

                if (_timeToHide <= 0f)
                {
                    _timeToHide = 0f;
                    Hide();
                }
            }
        }
    }
}