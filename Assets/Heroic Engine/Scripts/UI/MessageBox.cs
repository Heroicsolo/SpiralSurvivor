using HeroicEngine.Systems;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeroicEngine.UI
{
    public sealed class MessageBox : UIPart
    {
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] private Transform buttonsHolder;
        [SerializeField] private Button buttonPrefab;

        [Inject] private ITimeManager _timeManager;

        private bool _pauseGame;
        private readonly List<Button> _activeButtons = new();

        public void Show(string title, string message, bool pauseGame, params MessageBoxButton[] buttons)
        {
            base.Show();

            titleLabel.text = title;
            messageLabel.text = message;

            _pauseGame = pauseGame;

            ClearButtons();

            foreach (var button in buttons)
            {
                var newBtn = Instantiate(buttonPrefab, buttonsHolder);

                if (button.callback != null)
                {
                    newBtn.onClick.AddListener(button.callback);
                }

                newBtn.onClick.AddListener(Hide);

                var label = newBtn.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = button.text;
                }

                _activeButtons.Add(newBtn);
            }

            if (pauseGame)
            {
                _timeManager.PauseGame();
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (_pauseGame)
            {
                _timeManager.ResumeGame();
            }
        }

        private void ClearButtons()
        {
            foreach (var button in _activeButtons.ToArray())
            {
                Destroy(button.gameObject);
            }
            _activeButtons.Clear();
        }
    }

    public struct MessageBoxButton
    {
        public string text;
        public UnityAction callback;
    }
}
