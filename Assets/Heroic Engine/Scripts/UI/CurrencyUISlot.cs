using HeroicEngine.Components;
using HeroicEngine.Utils.Math;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public sealed class CurrencyUISlot : MonoBehaviour
    {
        [FormerlySerializedAs("icon")]
        [SerializeField] private Image _icon;
        [FormerlySerializedAs("amountLabel")]
        [SerializeField] private TextMeshProUGUI _amountLabel;

        private int _currAmount;

        public void SetData(Sprite icon, int amount)
        {
            _icon.sprite = icon;
            _amountLabel.text = $"{amount.ToShortenedNumber()}";
            _currAmount = amount;
        }

        public void SetAmount(int amount)
        {
            if (_currAmount != amount)
            {
                _amountLabel.GetComponent<LabelScaler>().SetLabelText(amount.ToShortenedNumber());
            }
            _currAmount = amount;
        }
    }
}