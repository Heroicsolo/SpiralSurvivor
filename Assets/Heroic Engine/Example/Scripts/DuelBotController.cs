using HeroicEngine.AI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroicEngine.Examples
{
    [RequireComponent(typeof(AIBrain))]
    internal class DuelBotController : DuelCharacterBase, IInjectable
    {
        private const string TASK_TYPE = "Duel_0";

        [Inject] private IEventsManager _eventsManager;
        [Inject] private DuelPlayerController _playerCharacter;

        private AIBrain _brain;
        private List<float> _duelParameters = new();
        private List<float> _duelParametersForPlayer = new();
        private readonly List<DuelTurnData> _myTurns = new();
        private readonly List<DuelTurnData> _playerTurns = new();

        public void PostInject()
        {
            _eventsManager.RegisterListener<int>("DuelPlayerSkillUsed", PlayerSkillUsed);
            _eventsManager.RegisterListener("DuelPlayerTurnEnd", StartTurn);
            _eventsManager.RegisterListener("DuelPlayerDied", OnPlayerDeath);
        }

        protected override void Start()
        {
            InjectionManager.RegisterObject(this);

            base.Start();

            _brain = GetComponent<AIBrain>();

            _duelParameters = new List<float>(new float[4 + Enum.GetValues(typeof(DuelParameter)).Length]);
            _duelParametersForPlayer = new List<float>(new float[4 + Enum.GetValues(typeof(DuelParameter)).Length]);
        }

        private void OnPlayerDeath()
        {
            // AI wins, teach its perceptron by his moves

            foreach (var turn in _myTurns)
            {
                _brain.SaveSolution(TASK_TYPE, turn.TurnParameters, (float)turn.SkillNumber / skills.Count);
            }

            ResetState();
        }

        protected override void OnSkillAnimEnd()
        {
            base.OnSkillAnimEnd();
            EndTurn();
        }

        protected override void Die()
        {
            base.Die();

            // Player wins, teach its perceptron by his moves

            foreach (var turn in _playerTurns)
            {
                _brain.SaveSolution(TASK_TYPE, turn.TurnParameters, (float)turn.SkillNumber / skills.Count, false);
            }

            _eventsManager.TriggerEvent("DuelBotDied");

            Invoke(nameof(ResetState), 3f);
        }

        private void ResetState()
        {
            ResetHealth();

            _currEnergy = energy;

            RefreshHPBar();
            RefreshEPBar();

            foreach (var skill in _skillsCds.Keys.ToArray())
            {
                if (_skillsCds[skill] > 0)
                {
                    _skillsCds[skill] = 0;
                }
            }

            if (ragdoll != null)
            {
                ragdoll.SetRagdollMode(false);
            }
        }

        private void EndTurn()
        {
            _eventsManager.TriggerEvent("DuelBotTurnEnd");
        }

        private void PlayerSkillUsed(int skillNumber)
        {
            _playerTurns.Add(new DuelTurnData
            {
                TurnParameters = new List<float>(_duelParametersForPlayer), SkillNumber = skillNumber
            });
        }

        private void StartTurn()
        {
            foreach (var skill in _skillsCds.Keys.ToArray())
            {
                if (_skillsCds[skill] > 0)
                {
                    _skillsCds[skill]--;
                }
            }

            _duelParametersForPlayer[(int)DuelParameter.EnemyHealth] = GetHPPercentage();
            _duelParametersForPlayer[(int)DuelParameter.EnemyEnergy] = GetEPPercentage();
            _duelParametersForPlayer[(int)DuelParameter.MyHealth] = _playerCharacter.GetHPPercentage();
            _duelParametersForPlayer[(int)DuelParameter.MyEnergy] = _playerCharacter.GetEPPercentage();
            _duelParametersForPlayer[(int)DuelParameter.IsEnemyStunned] = IsStunned() ? 1 : 0;

            for (var i = 0; i < skills.Count; i++)
            {
                _duelParametersForPlayer[(int)DuelParameter.IsEnemyStunned + i + 1] = _playerCharacter.IsSkillOnCd(i) ? 1 : 0;
            }

            if (IsStunned())
            {
                OnSkillAnimEnd();
                EndStun();
                return;
            }

            _duelParameters[(int)DuelParameter.EnemyHealth] = _playerCharacter.GetHPPercentage();
            _duelParameters[(int)DuelParameter.EnemyEnergy] = _playerCharacter.GetEPPercentage();
            _duelParameters[(int)DuelParameter.MyHealth] = GetHPPercentage();
            _duelParameters[(int)DuelParameter.MyEnergy] = GetEPPercentage();
            _duelParameters[(int)DuelParameter.IsEnemyStunned] = _playerCharacter.IsStunned() ? 1 : 0;

            for (var i = 0; i < skills.Count; i++)
            {
                _duelParameters[(int)DuelParameter.IsEnemyStunned + i + 1] = _skillsCds[skills[i]] > 0 ? 1 : 0;
            }

            if (_brain.FindSolution(TASK_TYPE, _duelParameters, out var skillNumber))
            {
                var idx = Mathf.RoundToInt(skillNumber * 3f);

                while (_skillsCds[skills[idx]] > 0)
                {
                    idx++;
                    if (idx == skills.Count)
                    {
                        idx = 0;
                    }
                }

                skills[idx].Perform(this, _playerCharacter);

                _skillsCds[skills[idx]] = skills[idx].Cooldown;

                _myTurns.Add(new DuelTurnData
                {
                    TurnParameters = new List<float>(_duelParameters), SkillNumber = idx
                });
            }
        }

        private enum DuelParameter
        {
            EnemyHealth,
            EnemyEnergy,
            MyHealth,
            MyEnergy,
            IsEnemyStunned
        }

        private struct DuelTurnData
        {
            public List<float> TurnParameters;
            public int SkillNumber;
        }
    }
}
