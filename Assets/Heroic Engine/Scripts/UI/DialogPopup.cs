using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public sealed class DialogPopup : UIPart, IPointerClickHandler
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] [Min(1)] private int textAppearanceSpeed = 10;
        [SerializeField] private DialogOptionButton optionButtonPrefab;
        [SerializeField] private Transform optionsHolder;

        private bool _isAppearing;
        private bool _canSkip = true;
        private float _maxTextAppearanceTime = 1f;
        private float _showTime = 5f;
        private UnityAction _closeCallback;
        private readonly List<DialogOptionButton> _optionButtons = new();

        public void ShowMessage(string message, Sprite avatarSprite, UnityAction closeCallback = null, float appearanceTime = 1f, float showTime = 5f)
        {
            gameObject.SetActive(true);
            ClearOptions();
            _canSkip = true;
            _maxTextAppearanceTime = appearanceTime;
            _closeCallback = closeCallback;
            _showTime = showTime;
            if (avatarSprite != null)
            {
                avatar.sprite = avatarSprite;
                avatar.gameObject.SetActive(true);
            }
            else
            {
                avatar.gameObject.SetActive(false);
            }
            StopAllCoroutines();
            StartCoroutine(TextAppearance(message));
        }

        public void ShowMessage(string message, Sprite avatarSprite, float appearanceTime = 1f, float showTime = 5f, params DialogOption[] dialogOptions)
        {
            gameObject.SetActive(true);
            ClearOptions();
            _canSkip = dialogOptions.Length == 0;
            _maxTextAppearanceTime = appearanceTime;
            _showTime = showTime;

            if (avatarSprite != null)
            {
                avatar.sprite = avatarSprite;
                avatar.gameObject.SetActive(true);
            }
            else
            {
                avatar.gameObject.SetActive(false);
            }

            foreach (var opt in dialogOptions)
            {
                var optionButton = Instantiate(optionButtonPrefab, optionsHolder);
                if (opt.callback != null)
                {
                    void FullCallback()
                    {
                        opt.callback();
                        Hide();
                    }

                    optionButton.SetData(opt.text, FullCallback);
                }
                else
                {
                    optionButton.SetData(opt.text, Hide);
                }
                //optionButton.gameObject.SetActive(false);
                _optionButtons.Add(optionButton);
            }

            StopAllCoroutines();
            StartCoroutine(TextAppearance(message));
        }

        public override void Hide()
        {
            base.Hide();

            _timeToHide = 0f;
            _closeCallback?.Invoke();
        }

        private void ClearOptions()
        {
            foreach (var option in _optionButtons.ToArray())
            {
                Destroy(option.gameObject);
            }

            _optionButtons.Clear();
        }

        private IEnumerator TextAppearance(string text)
        {
            var minInterval = _maxTextAppearanceTime / text.Length;
            var interval = Mathf.Min(minInterval, 1f / textAppearanceSpeed);

            messageLabel.text = "";

            var symbolsPrinted = 0;

            _isAppearing = true;

            do
            {
                messageLabel.text += text[symbolsPrinted];

                symbolsPrinted++;

                yield return new WaitForSeconds(interval);
            } while (symbolsPrinted < text.Length && _isAppearing);

            messageLabel.text = text;

            _timeToHide = _canSkip ? _showTime : 0f;

            _optionButtons.ForEach(button => button.gameObject.SetActive(true));

            _isAppearing = false;
        }

        public void Skip()
        {
            if (_isAppearing)
            {
                _isAppearing = false;
                _timeToHide = _canSkip ? _showTime : 0f;
            }
            else if (_canSkip)
            {
                Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Skip();
        }
    }

    public struct DialogOption
    {
        public string text;
        public UnityAction callback;
    }

    public enum DialogPopupMode
    {
        Fullscreen = 0,
        Corner = 1
    }
}
