using HeroicEngine.Components;
using HeroicEngine.Components.Combat;
using HeroicEngine.UI;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Examples
{
    internal class DuelCharacterBase : Hittable
    {
        private readonly int _animSkillHash = Animator.StringToHash("Skill");

        [SerializeField] private Animator animator;
        [SerializeField] protected Ragdoll ragdoll;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] protected List<DuelSkillInfo> skills;
        [SerializeField] private Transform particlesPivot;
        [SerializeField] private GameObject stunIndicator;
        [SerializeField] private Transform dialogTargetTransform;

        [Header("HP and EP balance")]
        [SerializeField] [Min(1f)] private float health = 100f;
        [SerializeField] [Min(1f)] protected float energy = 100f;

        [Header("HP and EP visuals")]
        [SerializeField] private ResourceBar hpBar;
        [SerializeField] private ResourceBar epBar;

        [SerializeField] private FlyUpText combatTextPrefab;
        [SerializeField] private Transform canvasTransform;

        protected float _currEnergy;
        protected bool _isStunned;
        protected readonly Dictionary<DuelSkillInfo, int> _skillsCds = new();

        public Transform CanvasTransform => canvasTransform;
        public Transform DialogTargetTransform => dialogTargetTransform;

        public void OnSkillUsed(DuelSkillInfo skillInfo)
        {
            _currEnergy -= skillInfo.UsageCost;
            _currEnergy = Mathf.Clamp(_currEnergy, 0f, energy);
            RefreshEPBar();
            if (skillInfo.AnimatorOverride != null)
            {
                animator.runtimeAnimatorController = skillInfo.AnimatorOverride;
                animator.Play(_animSkillHash);
            }
            if (skillInfo.Sound != null)
            {
                audioSource.PlayOneShot(skillInfo.Sound);
            }
            if (particlesPivot != null && skillInfo.Particles != null)
            {
                PoolSystem.GetInstanceAtPosition(skillInfo.Particles, skillInfo.Particles.GetName(), particlesPivot.position, particlesPivot.rotation);
            }
            Invoke(nameof(OnSkillAnimEnd), skillInfo.SkillAnimLength);
        }

        public float GetEPPercentage()
        {
            return _currEnergy / energy;
        }

        public bool IsStunned()
        {
            return _isStunned;
        }

        public void Stun()
        {
            _isStunned = true;
            stunIndicator.SetActive(true);
        }

        public void EndStun()
        {
            _isStunned = false;
            stunIndicator.SetActive(false);
        }

        protected virtual void Start()
        {
            _currEnergy = energy;
            _maxHealth = health;

            ResetHealth();

            SubscribeToDamageGot(damage =>
            {
                var ft = PoolSystem.GetInstanceAtPosition(combatTextPrefab, combatTextPrefab.GetName(), canvasTransform.position, canvasTransform);
                ft.SetColor(Color.red);
                ft.SetText($"-{Mathf.CeilToInt(damage)}");
                RefreshHPBar();
            });
            SubscribeToHealingGot(healing =>
            {
                var ft = PoolSystem.GetInstanceAtPosition(combatTextPrefab, combatTextPrefab.GetName(), canvasTransform.position, canvasTransform);
                ft.SetColor(Color.green);
                ft.SetText($"+{Mathf.CeilToInt(healing)}");
                RefreshHPBar();
            });
            SubscribeToDeath(Die);

            RefreshHPBar();
            RefreshEPBar();

            InitSkills();
        }

        protected virtual void InitSkills()
        {
            _skillsCds.Clear();

            skills.ForEach(skill =>
            {
                _skillsCds.Add(skill, 0);
            });
        }

        protected virtual void Die()
        {
            if (ragdoll != null)
            {
                ragdoll.SetRagdollMode(true);
            }
        }

        protected void RefreshHPBar()
        {
            hpBar.SetValue(_currHealth, _maxHealth);
        }

        protected void RefreshEPBar()
        {
            epBar.SetValue(_currEnergy, energy);
        }

        protected virtual void OnSkillAnimEnd()
        {
        }
    }
}
