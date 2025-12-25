using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Core.SoundPooling.Implement;
using _Project.Scripts.Core.SoundPooling.Interface;
using AYellowpaper.SerializedCollections;
using Sisus.Init;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using AudioType = _Project.Scripts.Core.SoundPooling.Interface.AudioType;

namespace _Project.Scripts.Core.SoundPooling
{
    public enum AudioOverridePolicy
    {
        DontPlayOnFull,
        OverrideFirst
    }

    [Service(typeof(AudioPooler), LoadScene = 0)]
    public class AudioPooler : MonoBehaviour
    {
        #region DebugProperties

        public int NumberOfActiveSounds { get; private set; }

        #endregion

        [SerializeField] private bool createBuffer;
        [SerializeField] private int bufferSize;

        [SerializeField] private AudioOverridePolicy audioOverridePolicy;

        [SerializeField] private SerializedDictionary<AudioType, int> maxAudioSources;
        [SerializeField] private SerializedDictionary<AudioType, AudioMixerGroup> audioMixerGroups;
        [SerializeField] private List<PooledAudioSource> inactiveSources;
        [SerializeField] private SerializedDictionary<AudioType, List<PooledAudioSource>> activeSources;

        private readonly HashSet<AudioType> _audioTypes = new();

        private void OnValidate()
        {
#if UNITY_EDITOR
            //Add new keys
            _audioTypes.Clear();
            _audioTypes.AddRange((AudioType[])Enum.GetValues(typeof(AudioType)));
            foreach (AudioType audioType in _audioTypes)
            {
                if (!maxAudioSources.ContainsKey(audioType))
                {
                    maxAudioSources.Add(audioType, 0);
                }

                if (!audioMixerGroups.ContainsKey(audioType))
                {
                    audioMixerGroups.Add(audioType, null);
                }
            }

            //Remove deleted Keys
            var keysToRemoveAudioSourceMax = new List<AudioType>();
            var keysToRemoveAudioMixerGroups = new List<AudioType>();

            foreach (var keyValue in maxAudioSources)
            {
                if (!_audioTypes.Contains(keyValue.Key))
                {
                    keysToRemoveAudioSourceMax.Add(keyValue.Key);
                }

                if (!audioMixerGroups.ContainsKey(keyValue.Key))
                {
                    keysToRemoveAudioMixerGroups.Add(keyValue.Key);
                }
            }

            foreach (var key in keysToRemoveAudioSourceMax)
            {
                maxAudioSources.Remove(key);
            }

            foreach (var key in keysToRemoveAudioMixerGroups)
            {
                audioMixerGroups.Remove(key);
            }
#endif
        }

        private void Awake()
        {
            //Buffer Audio Objects
            for (int i = 0; i < bufferSize; i++)
            {
                var audioSource = CreateAudioSource();
                audioSource.gameObject.SetActive(false);
                inactiveSources.Add(audioSource);
            }

            foreach (AudioType audioType in (AudioType[])Enum.GetValues(typeof(AudioType)))
            {
                activeSources[audioType] = new List<PooledAudioSource>();
            }
        }

        public AudioConfig2D New2DAudio(AudioClip clip)
        {
            return new AudioConfig2D(this, clip);
        }

        public AudioConfig3D New3DAudio(AudioClip clip)
        {
            return new AudioConfig3D(this, clip);
        }

        public IAudioPlayer Play(IAudioConfig audioConfig)
        {
            // check for capacity availability
            PooledAudioSource audioSource = null;
            if (activeSources[audioConfig.AudioType].Count >= maxAudioSources[audioConfig.AudioType])
            {
                int currentPriority = audioConfig.Priority;
                int minPriority = Int32.MaxValue;
                foreach (var activeAudio in activeSources[audioConfig.AudioType])
                {
                    if (activeAudio.Priority < minPriority && activeAudio.Priority < currentPriority)
                    {
                        minPriority = activeAudio.Priority;
                        audioSource = activeAudio;
                    }
                }

                if (audioSource != null)
                {
                    Debug.LogWarning($"AudioPooler of AudioType: {audioConfig.AudioType} is Full. " +
                                     $"Replaced playing {audioSource.Clip.name} with " +
                                     $"{audioConfig.Clip.name}");
                    audioSource.Stop();
                    audioSource.Initialize(audioConfig);
                    audioSource.gameObject.SetActive(true);
                    audioSource.Play();
                    return new AudioPlayer(audioSource);
                }

                switch (audioOverridePolicy)
                {
                    case AudioOverridePolicy.DontPlayOnFull:
                        Debug.LogWarning($"AudioPooler of AudioType: {audioConfig.AudioType} is Full. " +
                                         $"Skip playing {audioConfig.Clip.name}");
                        return new EmptyAudioPlayer();
                    case AudioOverridePolicy.OverrideFirst:
                        PooledAudioSource first = activeSources[audioConfig.AudioType]
                            .First(val => val.Priority == audioConfig.Priority);
                        Debug.LogWarning($"AudioPooler of AudioType: {audioConfig.AudioType} is Full. " +
                                         $"Replaced playing {first.Clip.name} with " +
                                         $"{audioConfig.Clip.name}");
                        first.Stop();
                        first.Initialize(audioConfig);
                        first.gameObject.SetActive(true);
                        first.Play();
                        
                        return new AudioPlayer(first);
                }
            }

            // Check for Audio Availability
            if (inactiveSources.Count > 0)
            {
                audioSource = inactiveSources[^1];
                inactiveSources.RemoveAt(inactiveSources.Count - 1);
                audioSource.Initialize(audioConfig);
                audioSource.gameObject.SetActive(true);
                activeSources[audioConfig.AudioType].Add(audioSource);
                NumberOfActiveSounds++;
                audioSource.Play();
                return new AudioPlayer(audioSource);
            }

            audioSource = CreateAudioSource();
            audioSource.Initialize(audioConfig);
            activeSources[audioConfig.AudioType].Add(audioSource);
            NumberOfActiveSounds++;
            audioSource.Play();
            return new AudioPlayer(audioSource);
        }

        public AudioMixerGroup GetMixerFor(AudioType audioType)
        {
            return audioMixerGroups[audioType];
        }

        public void ReturnToPool(PooledAudioSource pooledAudioSource)
        {
            activeSources[pooledAudioSource.AudioType].Remove(pooledAudioSource);
            pooledAudioSource.gameObject.SetActive(false);
            NumberOfActiveSounds--;
            inactiveSources.Add(pooledAudioSource);
        }

        private PooledAudioSource CreateAudioSource()
        {
            GameObject audioObject = new GameObject("AudioSource");
            audioObject.transform.SetParent(transform);

            PooledAudioSource audioComponent = audioObject.AddComponent<PooledAudioSource>();

            return audioComponent;
        }
    }
}
