using HeroicEngine.Utils.Math;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Systems.Audio
{
    public sealed class MusicPlayer : SystemBase, IMusicPlayer
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private List<MusicEntry> musicClips = new();
        [SerializeField] [Min(0f)] private float delayBetweenClips = 3f;

        private MusicEntryType _currentEntryType;
        private AudioClip _lastClip;
        private bool _isPlaying;

        [Serializable]
        public struct MusicEntry
        {
            public MusicEntryType entryType;
            public AudioClip[] musicClips;
        }

        private AudioClip[] GetClipsByEntryType(MusicEntryType type)
        {
            var idx = musicClips.FindIndex(c => c.entryType == type);

            return idx >= 0 ? musicClips[idx].musicClips : null;
        }

        public void Play(MusicEntryType entryType)
        {
            if (_isPlaying && entryType == _currentEntryType)
            {
                return;
            }
            _currentEntryType = entryType;
            PlayNextClip();
        }

        public void Stop()
        {
            _isPlaying = false;
            musicSource.Stop();
        }

        private void PlayNextClip()
        {
            var clips = GetClipsByEntryType(_currentEntryType);

            if (clips != null)
            {
                musicSource.clip = _lastClip ? clips.ToList().GetRandomElementExceptOne(_lastClip) : clips.ToList().GetRandomElement();
                musicSource.Play();
                _lastClip = musicSource.clip;
            }

            _isPlaying = true;
        }

        private void Update()
        {
            if (_isPlaying && !musicSource.isPlaying)
            {
                _isPlaying = false;
                Invoke(nameof(PlayNextClip), delayBetweenClips);
            }
        }
    }
}
