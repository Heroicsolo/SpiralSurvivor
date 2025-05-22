using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.UI;
using HeroicEngine.UI;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Examples
{
    internal sealed class DuelPlayerController : DuelCharacterBase, IInjectable
    {
        [SerializeField] private SkillButton skillButtonPrefab;
        [SerializeField] private Transform skillButtonsHolder;

        [Inject] private IEventsManager _eventsManager;
        [Inject] private IUIController _uiController;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private IQuestManager _questManager;
        [Inject] private IPlayerProgressionManager _playerProgressionManager;
        [Inject] private ICountdownController _countdownController;
        [Inject] private DuelBotController _botCharacter;

        private readonly Dictionary<DuelSkillInfo, SkillButton> _skillButtons = new();

        public void PostInject()
        {
            _eventsManager.RegisterListener("DuelBotTurnEnd", StartTurn);
            _eventsManager.RegisterListener("DuelBotDied", Win);

            skillButtonsHolder.gameObject.SetActive(false);

            _uiController.ShowMessageBox(_localizationManager.GetLocalizedString("DuelGameTitle"),
                _localizationManager.GetLocalizedString("DuelGameDescription"), "OK", ShowIntroDialog, true);
        }

        protected override void Start()
        {
            InjectionManager.RegisterObject(this);

            base.Start();
        }

        private void ShowIntroDialog()
        {
            var enemyTransform = _botCharacter.DialogTargetTransform;

            _uiController.ShowDialog(DialogPopupMode.Fullscreen, _localizationManager.GetLocalizedString("DuelIntroDialogMsg"), enemyTransform, 1f, 1f,
                new DialogOption
                {
                    text = _localizationManager.GetLocalizedString("DuelIntroDialogOption1"), callback = StartGame
                });
        }

        private void StartGame()
        {
            _countdownController.StartCountdown(3, null, ShowSkills);
        }

        private void ShowSkills()
        {
            skillButtonsHolder.gameObject.SetActive(true);
        }

        public bool IsSkillOnCd(int idx)
        {
            return _skillsCds[skills[idx]] > 0;
        }

        protected override void InitSkills()
        {
            base.InitSkills();

            skills.ForEach(skill =>
            {
                var button = Instantiate(skillButtonPrefab, skillButtonsHolder);
                button.Setup(skill.Icon, skill.UsageCost.ToString(), () =>
                {
                    if (_currEnergy >= skill.UsageCost && !_isStunned)
                    {
                        skill.Perform(this, _botCharacter);
                        _skillsCds[skill] = skill.Cooldown;
                        _eventsManager.TriggerEvent("DuelPlayerSkillUsed", skills.FindIndex(s => s == skill));
                        button.SetCooldown(1f);
                    }
                });
                _skillButtons.Add(skill, button);
            });
        }

        protected override void OnSkillAnimEnd()
        {
            base.OnSkillAnimEnd();
            EndTurn();
        }

        protected override void Die()
        {
            base.Die();
            _eventsManager.TriggerEvent("DuelPlayerDied");
            Invoke(nameof(Loss), 3f);
        }

        private void Win()
        {
            ResetState();
            _uiController.ShowUIParts(UIPartType.VictoryScreen);
            _questManager.AddProgress(QuestTaskType.GameWon, 1);
            _playerProgressionManager.AddExperience(25);
        }

        private void Loss()
        {
            ResetState();
            _uiController.ShowUIParts(UIPartType.FailScreen);
        }

        private void StartTurn()
        {
            foreach (var skillBtn in _skillButtons)
            {
                if (_skillsCds[skillBtn.Key] > 0)
                {
                    _skillsCds[skillBtn.Key]--;
                    skillBtn.Value.SetCooldown((float)_skillsCds[skillBtn.Key] / skillBtn.Key.Cooldown);
                }
            }

            if (_isStunned)
            {
                EndTurn();
                EndStun();
            }
        }

        private void ResetState()
        {
            ResetHealth();

            _currEnergy = energy;

            RefreshHPBar();
            RefreshEPBar();

            foreach (var skillBtn in _skillButtons)
            {
                if (_skillsCds[skillBtn.Key] > 0)
                {
                    _skillsCds[skillBtn.Key] = 0;
                    skillBtn.Value.SetCooldown(0f);
                }
            }

            if (ragdoll != null)
            {
                ragdoll.SetRagdollMode(false);
            }
        }

        private void EndTurn()
        {
            _eventsManager.TriggerEvent("DuelPlayerTurnEnd");
        }

        private void OnDestroy()
        {
            _countdownController.CancelCountdown();
        }
    }
}
