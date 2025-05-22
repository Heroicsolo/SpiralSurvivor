using HeroicEngine.AI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Utils.Math;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HeroicEngine.Examples
{
    [RequireComponent(typeof(AIBrain))]
    internal sealed class SampleTicTacToeBot : MonoBehaviour
    {
        private const string TASK_TYPE = "TicTacToe_0";

        [SerializeField] private TicTacToeController ticTacToeController;
        [SerializeField] private TextMeshProUGUI perceptronLabel;

        [Inject] private IEventsManager _eventsManager;

        private readonly List<TurnData> _turnsHistory = new();
        private readonly List<TurnData> _playerTurnsHistory = new();

        private int _nextCellIdx;
        private AIBrain _brain;
        private Perceptron _perceptron;
        private int _turnNumber;

        private void Awake()
        {
            _brain = GetComponent<AIBrain>();
            _brain.SubscribeToBrainInit(PrintPerceptronState);
        }

        private void Start()
        {
            InjectionManager.InjectTo(this);

            _eventsManager.RegisterListener<int>("TicTac_Player_Turn_End", OnPlayerTurnEnd);
            _eventsManager.RegisterListener("TicTac_AI_Win", OnWin);
            _eventsManager.RegisterListener("TicTac_Player_Win", OnLoss);
            _eventsManager.RegisterListener("TicTac_Tie", OnTie);
            _eventsManager.RegisterListener("TicTac_Reset", OnFieldReset);

            OnFieldReset();
        }

        private void PrintPerceptronState()
        {
            perceptronLabel.text = "";
            _perceptron ??= _brain.GetPerceptronByTask(TASK_TYPE);

            if (_perceptron != null)
            {
                for (var i = 0; i < _perceptron.InputsNumber; i++)
                {
                    for (var k = 0; k < _perceptron.NeuronsNumber; k++)
                    {
                        if (_perceptron.Weights[k][i] >= 0f)
                        {
                            perceptronLabel.text += _perceptron.Weights[k][i].ToRoundedString(2) + "  | ";
                        }
                        else
                        {
                            perceptronLabel.text += _perceptron.Weights[k][i].ToRoundedString(2) + " | ";
                        }
                    }

                    perceptronLabel.text += "\n";
                }
            }
        }

        private void OnDisable()
        {
            _eventsManager.UnregisterListener<int>("TicTac_Player_Turn_End", OnPlayerTurnEnd);
            _eventsManager.UnregisterListener("TicTac_AI_Win", OnWin);
            _eventsManager.UnregisterListener("TicTac_Player_Win", OnLoss);
            _eventsManager.UnregisterListener("TicTac_Tie", OnTie);
        }

        private void OnFieldReset()
        {
            var fieldState = ticTacToeController.GetFieldStateAsFloats();

            _nextCellIdx = UnityEngine.Random.Range(0, 9);

            _turnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState), cellIdx = _nextCellIdx
            });

            // Wait 1 sec. and perform AI turn on field
            Invoke(nameof(DoTurn), 1f);
        }

        private void ResetState()
        {
            _turnsHistory.Clear();
            _playerTurnsHistory.Clear();
            _turnNumber = 0;
        }

        private void OnWin()
        {
            //If AI bot wins, we train his perceptron by all his own turns

            _turnsHistory.ForEach(turn =>
            {
                _brain.SaveSolution(TASK_TYPE, turn.fieldState, turn.cellIdx / 8f);
            });

            // And forget last player turn as failed one

            var lastPlayerTurn = _playerTurnsHistory[^1];
            _brain.ForgetSolution(TASK_TYPE, lastPlayerTurn.fieldState, lastPlayerTurn.cellIdx / 8f);

            PrintPerceptronState();

            ResetState();
        }

        private void OnLoss()
        {
            //If player wins, we train his perceptron by all player's turns

            _playerTurnsHistory.ForEach(turn =>
            {
                _brain.SaveSolution(TASK_TYPE, turn.fieldState, turn.cellIdx / 8f, false);
            });

            // And forget our last turn as failed one

            var lastTurn = _turnsHistory[^1];
            _brain.ForgetSolution(TASK_TYPE, lastTurn.fieldState, lastTurn.cellIdx / 8f);

            PrintPerceptronState();

            ResetState();
        }

        private void OnTie()
        {
            ResetState();
        }

        private void OnPlayerTurnEnd(int cellIdx)
        {
            var fieldState = ticTacToeController.GetFieldStateAsFloats();

            // Getting cell index to do AI turn on board
            if (_brain.FindSolution(TASK_TYPE, fieldState, out var solution))
            {
                _nextCellIdx = Mathf.FloorToInt(Mathf.Clamp01(solution * 8));
            }

            // If chosen cell isn't empty, we select random one
            if (fieldState[_nextCellIdx] != 0f)
            {
                var indices = new List<int>();
                for (var i = 0; i < fieldState.Count; i++)
                {
                    if (fieldState[i] == 0f)
                    {
                        indices.Add(i);
                    }
                }
                _nextCellIdx = indices.GetRandomElement();
            }

            // We survived 2 turns, so our last turn was effective, learn it
            if (_turnNumber > 1)
            {
                var lastTurn = _turnsHistory[^1];
                _brain.SaveSolution(TASK_TYPE, lastTurn.fieldState, lastTurn.cellIdx / 8f, false);
                PrintPerceptronState();
            }

            // Add this turn to our turns history
            _turnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState), cellIdx = _nextCellIdx
            });

            // Invert field state to "look" at field as player
            for (var i = 0; i < fieldState.Count; i++)
            {
                fieldState[i] *= -1;
            }

            // Add this turn to our turns history
            _playerTurnsHistory.Add(new TurnData
            {
                fieldState = new List<float>(fieldState), cellIdx = cellIdx
            });

            // Wait 1 sec. and perform AI turn on field
            Invoke(nameof(DoTurn), 1f);
        }

        private void DoTurn()
        {
            ticTacToeController.SetField(_nextCellIdx, TicTacToeController.TicTacToeSymbol.X);
            _turnNumber++;
        }

        [Serializable]
        private struct TurnData
        {
            public List<float> fieldState;
            public int cellIdx;
        }
    }
}
