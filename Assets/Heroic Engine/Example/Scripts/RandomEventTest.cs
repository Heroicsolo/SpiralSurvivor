using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Utils;
using HeroicEngine.Systems.DI;
using TMPro;
using UnityEngine;

namespace HeroicEngine.Examples
{
    internal sealed class RandomEventTest : MonoBehaviour
    {
        [SerializeField] private RandomEventInfo randEvent;
        [SerializeField] private TextMeshProUGUI debugLabel;
        [SerializeField] private TextMeshProUGUI buttonLabel;

        [Inject] private IRandomEventsManager _randomEventsManager;
        [Inject] private ILocalizationManager _localizationManager;

        private int _attemptNumber = 1;

        private void Start()
        {
            InjectionManager.InjectTo(this);
            debugLabel.text = "";
            _randomEventsManager.ResetEventChance(randEvent.EventType);
            buttonLabel.text = _localizationManager.GetLocalizedString("RandEventTest", Mathf.FloorToInt(100f * _randomEventsManager.GetEventChance(randEvent.EventType)));
        }

        public void DoAttempt()
        {
            if (_randomEventsManager.DoEventAttempt(randEvent))
            {
                debugLabel.text = $"{_localizationManager.GetLocalizedString("Attempt", _attemptNumber)}: "
                    + _localizationManager.GetLocalizedString("Success").ToColorizedString(Color.green);
                _attemptNumber = 1;
            }
            else
            {
                debugLabel.text = $"{_localizationManager.GetLocalizedString("Attempt", _attemptNumber)}: "
                    + _localizationManager.GetLocalizedString("Fail").ToColorizedString(Color.red);
                _attemptNumber++;
            }

            buttonLabel.text = _localizationManager.GetLocalizedString("RandEventTest", Mathf.FloorToInt(100f * _randomEventsManager.GetEventChance(randEvent.EventType)));
        }
    }
}