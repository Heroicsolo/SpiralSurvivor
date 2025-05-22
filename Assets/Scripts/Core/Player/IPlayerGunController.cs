using HeroicEngine.Systems.DI;

namespace Heroicsolo.SpiralSurvivor.Core.Player
{
    public interface IPlayerGunController : ISystem
    {
        void StartShooting();
        void EndShooting();
    }
}