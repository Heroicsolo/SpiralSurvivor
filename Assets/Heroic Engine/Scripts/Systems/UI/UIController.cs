using HeroicEngine.Components;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.UI;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using HeroicEngine.Enums;
using UnityEngine.UI;
using HeroicEngine.Systems.Localization;
using UnityEditor;

namespace HeroicEngine.Systems.UI
{
    public sealed class UIController : SystemBase, IUIController
    {
        [SerializeField] private CurrencyUISlot currencySlotPrefab;
        [SerializeField] private Transform currenciesHolder;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private ResourceBar expBar;
        [SerializeField] private LabelScaler announcementLabel;
        [SerializeField] private TextMeshProUGUI loadingLabel;
        [SerializeField] private Image loadingBar;

        [Inject] private ISoundsManager _soundsManager;
        [Inject] private ICurrenciesManager _currenciesManager;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private ICameraController _cameraController;

        private readonly Dictionary<UIPartType, List<UIPart>> _uiPartsBindings = new();
        private readonly Dictionary<CurrencyType, CurrencyUISlot> _currencySlots = new();
        private float _announcementTimeLeft;

        public void UpdateLoadingPanel(float progress, string text)
        {
            loadingLabel.text = _localizationManager.GetLocalizedString(text, Mathf.CeilToInt(100f * progress));
            loadingBar.fillAmount = progress;
        }

        public void UpdateExperiencePanel(int level, int currExp, int maxExp)
        {
            levelLabel.text = level.ToString();
            expBar.SetValue(currExp, maxExp);
        }

        public void UpdateCurrencySlot(CurrencyType currencyType, int amount)
        {
            if (_currenciesManager == null)
            {
                InjectionManager.InjectTo(this);
            }

            if (_currencySlots.TryGetValue(currencyType, out var slot))
            {
                slot.SetAmount(amount);
            }
            else
            {
                var currencyUISlot = Instantiate(currencySlotPrefab, currenciesHolder);

                Sprite icon = null;

                if (_currenciesManager != null && _currenciesManager.GetCurrencyInfo(currencyType, out var info))
                {
                    icon = info.Icon;
                }

                currencyUISlot.SetData(icon, amount);

                _currencySlots.Add(currencyType, currencyUISlot);
            }
        }

        public void ShowMessageBox(string title, string message, string buttonText, UnityAction buttonCallback, bool pauseGame = false)
        {
            var messageBox = _uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, new MessageBoxButton
                {
                    callback = buttonCallback, text = buttonText
                });
            }
        }

        public void ShowMessageBox(string title, string message, bool pauseGame, params MessageBoxButton[] buttons)
        {
            var messageBox = _uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, buttons);
            }
        }

        public void ShowMessageBox(string title, string message, bool pauseGame = false)
        {
            var messageBox = _uiPartsBindings[UIPartType.MessageBox].First() as MessageBox;

            if (messageBox != null)
            {
                messageBox.Show(title, message, pauseGame, new MessageBoxButton
                {
                    text = "OK", callback = null
                });
            }
        }

        public void HideCurrentDialog()
        {
            HideUIParts(UIPartType.FullscreenDialogPopup);
            HideUIParts(UIPartType.CornerDialogPopup);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, UnityAction closeCallback = null, float appearanceTime = 1f, float showTime = 5f)
        {
            var partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            var dialogPopup = _uiPartsBindings[partType].First() as DialogPopup;
            
            _cameraController.LookAtFromPos(targetTransform.position + targetDistance * targetTransform.forward, targetTransform);
            
            if (dialogPopup != null)
            {
                void FullCallback()
                {
                    _cameraController.ResetState();
                    closeCallback?.Invoke();
                }

                dialogPopup.ShowMessage(message, null, FullCallback, appearanceTime, showTime);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, UnityAction closeCallback = null, float appearanceTime = 1f, float showTime = 5f)
        {
            var partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            var dialogPopup = _uiPartsBindings[partType].First() as DialogPopup;

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, avatarSprite, closeCallback, appearanceTime, showTime);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f, float showTime = 5f)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, avatarSprite, closeCallback, appearanceTime, showTime);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, UnityAction closeCallback = null, float appearanceTime = 1f, float showTime = 5f)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, closeCallback, appearanceTime, showTime);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            ShowDialog(dialogPopupMode, message, avatarSprite, appearanceTime, 5f, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, float appearanceTime = 1f, float showTime = 5f, params DialogOption[] dialogOptions)
        {
            var partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            var dialogPopup = _uiPartsBindings[partType].First() as DialogPopup;

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, avatarSprite, appearanceTime, showTime, dialogOptions);
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, appearanceTime, 5f, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, float appearanceTime = 1f, float showTime = 5f, params DialogOption[] dialogOptions)
        {
            var partType = dialogPopupMode == DialogPopupMode.Fullscreen ? UIPartType.FullscreenDialogPopup : UIPartType.CornerDialogPopup;
            var dialogPopup = _uiPartsBindings[partType].First() as DialogPopup;
            
            _cameraController.LookAtFromPos(targetTransform.position + targetDistance * targetTransform.forward, targetTransform);

            var modifiedOptions = new List<DialogOption>();

            foreach (var option in dialogOptions.ToArray())
            {
                modifiedOptions.Add(new DialogOption
                {
                    text = option.text, callback = FullCallback
                });
                continue;

                void FullCallback()
                {
                    _cameraController.ResetState();
                    option.callback();
                }
            }

            if (dialogPopup != null)
            {
                dialogPopup.ShowMessage(message, null, appearanceTime, showTime, modifiedOptions.ToArray());
            }
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, avatarSprite, appearanceTime, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Sprite avatarSprite, AudioClip sound, float appearanceTime = 1f, float showTime = 5f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, avatarSprite, appearanceTime, showTime, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, float appearanceTime = 1f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, appearanceTime, dialogOptions);
        }

        public void ShowDialog(DialogPopupMode dialogPopupMode, string message, Transform targetTransform, float targetDistance, AudioClip sound, float appearanceTime = 1f, float showTime = 5f, params DialogOption[] dialogOptions)
        {
            if (sound != null)
            {
                _soundsManager.PlayClip(sound);
            }

            ShowDialog(dialogPopupMode, message, targetTransform, targetDistance, appearanceTime, showTime, dialogOptions);
        }

        public void ShowAnnouncement(string message, float showTime = 3f)
        {
            if (!string.IsNullOrEmpty(message))
            {
                announcementLabel.gameObject.SetActive(true);
                announcementLabel.SetLabelText(message);
                _announcementTimeLeft = showTime;
            }
        }

        public void HideAnnouncement()
        {
            announcementLabel.gameObject.SetActive(false);
        }

        public void RegisterUIPart(UIPart part)
        {
            if (!_uiPartsBindings.ContainsKey(part.PartType))
            {
                _uiPartsBindings.Add(part.PartType, new List<UIPart>
                {
                    part
                });
            }
            else if (!_uiPartsBindings[part.PartType].Contains(part))
            {
                _uiPartsBindings[part.PartType].Add(part);
            }
        }

        public void UnregisterUIPart(UIPart part)
        {
            if (_uiPartsBindings.TryGetValue(part.PartType, out var binding))
            {
                binding.Remove(part);
            }
        }

        public List<UIPart> GetUIPartsOfType(UIPartType type)
        {
            return _uiPartsBindings[type];
        }

        public void ShowUIParts(UIPartType uiPart)
        {
            _uiPartsBindings[uiPart].ForEach(p => p.Show());
        }

        public void HideUIParts(UIPartType uiPart)
        {
            _uiPartsBindings[uiPart].ForEach(p => p.Hide());
        }

        public void OnExitButtonClicked()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void Awake()
        {
            FindUIParts();
        }

        private void FindUIParts()
        {
            var uiParts = transform.GetComponentsInChildren<UIPart>(true).ToList();

            uiParts.ForEach(RegisterUIPart);
        }

        private void Update()
        {
            if (announcementLabel.gameObject.activeSelf && _announcementTimeLeft > 0f)
            {
                _announcementTimeLeft -= Time.deltaTime;

                if (_announcementTimeLeft <= 0f)
                {
                    HideAnnouncement();
                }
            }
        }
    }
}
