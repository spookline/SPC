using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spookline.SPC.Registry;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Spookline.SPC.Audio {
    [CreateAssetMenu(fileName = "AudioDefinition", menuName = "Spookline/AudioDefinition", order = 1)]
    public class AudioDefinition : RegistryObject {

        public AudioMixerGroup group;

        [BoxGroup("Default Audio Options")]
        [HideLabel]
        public AudioOptions options = AudioOptions.Default;

        [Title("Provider")]
        [PolymorphicDrawerSettings(ShowBaseType = false)]
        [HideLabel]
        [InlineProperty]
        public IAudioSourceProvider provider;

    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public struct AudioOptions {

        public static readonly AudioOptions Default = new() {
            loop = false,
            volume = 1f,
            pitch = 1f,
            minDistance = 1f,
            maxDistance = 15f
        };

        public bool loop;
        public float volume;
        public float pitch;
        public float minDistance;
        public float maxDistance;

        public AudioOptions Volume(float volume) {
            this.volume = volume;
            return this;
        }

        public AudioOptions Loop(bool loop = true) {
            this.loop = loop;
            return this;
        }

        public AudioOptions Pitch(float pitch) {
            this.pitch = pitch;
            return this;
        }

        public AudioOptions MinDistance(float minDistance) {
            this.minDistance = minDistance;
            return this;
        }

        public AudioOptions MaxDistance(float maxDistance) {
            this.maxDistance = maxDistance;
            return this;
        }


        public readonly void ApplyTo(AudioSource source) {
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
        }

    }

    [Serializable]
    public class SerializedAudioJob {

        public string guid;
        public int data;
        public AudioOptions options;


        public AudioJob ToAudioJob() {
            var def = SpookAudioRegistry.Instance.GuidLookup[guid];
            return new AudioJob(def, data, options);
        }

    }

    public readonly struct AudioJob {

        public readonly AudioDefinition definition;
        public readonly int data;
        public readonly AudioOptions options;

        public AudioJob(AudioDefinition definition, int data, AudioOptions options) {
            this.definition = definition;
            this.data = data;
            this.options = options;
        }

        public AudioJob WithOptions(AudioOptions newOptions) {
            return new AudioJob(definition, data, newOptions);
        }

        public AudioJob With(Func<AudioOptions, AudioOptions> modifier) {
            var newOptions = modifier(options);
            return new AudioJob(definition, data, newOptions);
        }

        public SerializedAudioJob Serialize() {
            return new SerializedAudioJob {
                guid = definition.assetGuid,
                data = data,
                options = options
            };
        }

        public void Play() {
            SpookAudioModule.Instance.Play(this).Forget();
        }

        public void PlayAt(Vector3 position) {
            SpookAudioModule.Instance.Play(this, position).Forget();
        }

        public void PlayTracked(Transform transform) {
            SpookAudioModule.Instance.Play(this, transform).Forget();
        }

        public UniTask<AudioClip> GetClipAsync() {
            return SpookAudioModule.Instance.GetClip(this);
        }

    }


    public interface IAudioSourceProvider {

        public bool IsLoaded { get; }
        public UniTask Load();
        public UniTask Unload();
        public AudioJob CreateJob(AudioDefinition definition);
        public AudioClip GetClip(AudioJob job);
        public void Apply(AudioHandle handle, AudioJob job);

    }

    [Serializable]
    public class RangeAudioSourceProvider : IAudioSourceProvider {

        public List<AudioAssetEntry> clips = new();

        [NonSerialized]
        private List<AsyncOperationHandle> _handles;

        [NonSerialized]
        private List<AudioClip> _loadedClips;

        public bool IsLoaded { get; private set; } // ReSharper disable Unity.PerformanceAnalysis
        public async UniTask Load() {
            if (IsLoaded) {
                Debug.LogWarning("AudioSourceProvider is already loaded.");
                return;
            }

            _handles = new List<AsyncOperationHandle>();
            _loadedClips = new List<AudioClip>();

            foreach (var entry in clips) {
                var handle = Addressables.LoadAssetAsync<AudioClip>(entry.clip.AssetGUID);
                Debug.Log($"Loading audio clip: {handle}, {_handles?.ToString() ?? "null"}");
                var clip = await handle;
                if (clip == null) {
                    Debug.LogError($"Failed to load audio clip: {entry.clip}");
                    continue;
                }

                _handles!.Add(handle);
                _loadedClips!.Add(clip);
            }

            IsLoaded = true;
        }

        public UniTask Unload() {
            foreach (var handle in _handles)
                if (handle.IsValid())
                    Addressables.Release(handle);

            _loadedClips?.Clear();
            _handles?.Clear();
            IsLoaded = false;
            return UniTask.CompletedTask;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public AudioJob CreateJob(AudioDefinition definition) {
            switch (clips.Count) {
                case 1:
                    return new AudioJob(definition, 0, definition.options);
                case > 1: {
                    var randomIndex = Random.Range(0, _loadedClips.Count);
                    return new AudioJob(definition, randomIndex, definition.options);
                }
                default:
                    Debug.LogError("No audio clips available in RangeAudioSourceProvider.");
                    return default;
            }
        }

        public AudioClip GetClip(AudioJob job) {
            return _loadedClips[job.data];
        }

        public void Apply(AudioHandle handle, AudioJob job) {
            handle.source.clip = _loadedClips[job.data];
            job.options.ApplyTo(handle.source);
        }

    }

    [Serializable]
    public struct AudioAssetEntry {

        [HideLabel]
        public AssetReferenceT<AudioClip> clip;

    }
}