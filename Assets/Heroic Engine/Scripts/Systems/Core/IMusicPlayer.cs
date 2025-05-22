using HeroicEngine.Enums;
using HeroicEngine.Systems.DI;

namespace HeroicEngine.Systems.Audio
{
    public interface IMusicPlayer : ISystem
    {
        void Play(MusicEntryType entryType);
        void Stop();
    }
}