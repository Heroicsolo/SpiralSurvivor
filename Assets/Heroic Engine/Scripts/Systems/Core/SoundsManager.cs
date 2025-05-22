using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Audio
{
    public class SoundsManager : SystemBase, ISoundsManager
    {
        [SerializeField] private AudioSource audioSource;

        private readonly List<AudioSource> _temporaryAudioSources = new();

        public void PlayClip(AudioClip clip)
        {
            GetFreeAudioSource().PlayOneShot(clip);
        }

        public void StopAllSounds()
        {
            audioSource.Stop();
            _temporaryAudioSources.ForEach(a => a.Stop());
        }

        private AudioSource GetFreeAudioSource()
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }

            var freeSource = _temporaryAudioSources.Find(a => !a.isPlaying);

            if (freeSource == null)
            {
                freeSource = Instantiate(audioSource, audioSource.transform.parent);
                _temporaryAudioSources.Add(freeSource);
            }

            return freeSource;
        }
    }
}