using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class SkillButton : MonoBehaviour
    {
        [FormerlySerializedAs("icon")]
        [SerializeField] private Image _icon;
        [FormerlySerializedAs("costLabel")]
        [SerializeField] private TextMeshProUGUI _costLabel;
        [FormerlySerializedAs("cdIndicator")]
        [SerializeField] private Image _cdIndicator;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Setup(Sprite icon, string costText, UnityAction onClickedCallback)
        {
            _button ??= GetComponent<Button>();

            _icon.sprite = icon;
            _costLabel.text = costText;
            _cdIndicator.fillAmount = 0f;

            _button.onClick.AddListener(onClickedCallback);
        }

        public void SetCooldown(float cooldownPercent)
        {
            _cdIndicator.fillAmount = Mathf.Clamp01(cooldownPercent);
            _button.interactable = _cdIndicator.fillAmount == 0f;
        }
    }
}