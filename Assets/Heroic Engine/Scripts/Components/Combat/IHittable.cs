using UnityEngine;
using UnityEngine.Events;
namespace HeroicEngine.Components.Combat
{
    public interface IHittable
    {
        TeamType TeamType { get; }
        
        void SetTeam(TeamType teamType);
        Transform GetHitTransform();
        void GetDamage(float damage);
        void Heal(float amount);
        void Kill();
        void ResetHealth();
        bool IsDead();
        float GetHPPercentage();
        float GetHP();
        float GetMaxHP();
        void SubscribeToDamageGot(UnityAction<float> onDamageGot);
        void SubscribeToHealingGot(UnityAction<float> onHealingGot);
        void SubscribeToDeath(UnityAction onDeath);
        void RemoveAllListeners();
    }
}