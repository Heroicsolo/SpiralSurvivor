using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HeroicEngine.Enums;

namespace HeroicEngine.Examples
{
    internal sealed class TicTacToeController : MonoBehaviour
    {
        private const string X = "X";
        private const string O = "O";
        private const string YOUR_TURN = "Your turn!";
        private const string AI_TURN = "AI turn...";
        private const string YOU_WIN = "You win!";
        private const string AI_WINS = "AI wins!";
        private const string TIE = "Tie!";

        // Check rows, columns, and diagonals
        private readonly int[,] _winPatterns = new int[,]
        {
            {
                0, 1, 2
            }, // Row 1
            {
                3, 4, 5
            }, // Row 2
            {
                6, 7, 8
            }, // Row 3
            {
                0, 3, 6
            }, // Column 1
            {
                1, 4, 7
            }, // Column 2
            {
                2, 5, 8
            }, // Column 3
            {
                0, 4, 8
            }, // Diagonal 1
            {
                2, 4, 6
            } // Diagonal 2
        };

        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private List<Button> buttons = new();

        [Inject] private IEventsManager _eventsManager;
        [Inject] private IQuestManager _questManager;
        [Inject] private IPlayerProgressionManager _playerProgressionManager;

        private readonly List<TicTacToeSymbol> _fieldState = new();
        private readonly List<TextMeshProUGUI> _buttonsLabels = new();
        private bool _gameOver;
        private bool _aiTurn;

        public List<float> GetFieldStateAsFloats()
        {
            var result = new List<float>();

            _fieldState.ForEach(s =>
            {
                switch (s)
                {
                    case TicTacToeSymbol.None: result.Add(0f); break;
                    case TicTacToeSymbol.O: result.Add(-1f); break;
                    case TicTacToeSymbol.X: result.Add(1f); break;
                }
            });

            return result;
        }

        public void SetField(int cell, TicTacToeSymbol symbol)
        {
            if (_gameOver)
            {
                return;
            }

            _fieldState[cell] = symbol;

            switch (symbol)
            {
                case TicTacToeSymbol.None:
                    _buttonsLabels[cell].text = string.Empty;
                    break;
                case TicTacToeSymbol.X:
                    _buttonsLabels[cell].text = X;
                    statusLabel.text = YOUR_TURN;
                    _aiTurn = false;
                    break;
                case TicTacToeSymbol.O:
                    _buttonsLabels[cell].text = O;
                    _aiTurn = true;
                    break;
            }

            CheckField();
        }

        private void CheckField()
        {
            for (var i = 0; i < _winPatterns.GetLength(0); i++)
            {
                if (_fieldState[_winPatterns[i, 0]] == TicTacToeSymbol.X &&
                    _fieldState[_winPatterns[i, 1]] == TicTacToeSymbol.X &&
                    _fieldState[_winPatterns[i, 2]] == TicTacToeSymbol.X)
                {
                    _gameOver = true;
                    statusLabel.text = AI_WINS;
                    _eventsManager.TriggerEvent("TicTac_AI_Win");
                    Invoke(nameof(ResetField), 3f);
                    return;
                }

                if (_fieldState[_winPatterns[i, 0]] == TicTacToeSymbol.O &&
                    _fieldState[_winPatterns[i, 1]] == TicTacToeSymbol.O &&
                    _fieldState[_winPatterns[i, 2]] == TicTacToeSymbol.O)
                {
                    _gameOver = true;
                    statusLabel.text = YOU_WIN;
                    _eventsManager.TriggerEvent("TicTac_Player_Win");
                    _questManager.AddProgress(QuestTaskType.GameWon, 1);
                    _playerProgressionManager.AddExperience(10);
                    Invoke(nameof(ResetField), 3f);
                    return;
                }
            }

            foreach (var s in _fieldState)
            {
                if (s == TicTacToeSymbol.None)
                    return;
            }

            _gameOver = true;
            statusLabel.text = TIE;
            _eventsManager.TriggerEvent("TicTac_Tie");
            Invoke(nameof(ResetField), 3f);
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);

            for (var i = 0; i < buttons.Count; i++)
            {
                var index = i;
                buttons[i].onClick.AddListener(() => OnButtonClick(index));
                _buttonsLabels.Add(buttons[i].GetComponentInChildren<TextMeshProUGUI>());
                _fieldState.Add(TicTacToeSymbol.None);
            }

            ResetField();
        }

        private void OnButtonClick(int index)
        {
            if (_fieldState[index] == TicTacToeSymbol.None && !_gameOver && !_aiTurn)
            {
                SetField(index, TicTacToeSymbol.O);

                if (!_gameOver)
                {
                    _eventsManager.TriggerEvent("TicTac_Player_Turn_End", index);
                    statusLabel.text = AI_TURN;
                }
            }
        }

        private void ResetField()
        {
            _buttonsLabels.ForEach(lbl => lbl.text = string.Empty);
            for (var i = 0; i < _fieldState.Count; i++)
            {
                _fieldState[i] = TicTacToeSymbol.None;
            }
            statusLabel.text = AI_TURN;
            _gameOver = false;
            _aiTurn = true;
            _eventsManager.TriggerEvent("TicTac_Reset");
        }

        public enum TicTacToeSymbol
        {
            None,
            O,
            X
        }
    }
}
