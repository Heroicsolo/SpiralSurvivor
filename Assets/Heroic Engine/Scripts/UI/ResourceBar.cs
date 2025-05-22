using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public sealed class ResourceBar : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] [Min(0f)] private float valueChangeTime = 0.5f;

        private float _currValue;
        
        public void SetValue(float value, float maxValue)
        {
            // Animate value change
            DOTween.To(() => bar.fillAmount, x => bar.fillAmount = x, value / maxValue, valueChangeTime);
            DOTween.To(() => _currValue, x =>
            {
                _currValue = x;
                valueLabel.text = $"{Mathf.CeilToInt(_currValue)}/{Mathf.CeilToInt(maxValue)}";
            }, value, valueChangeTime);
        }
    }
}