using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Spookline.SPC.Audio {
    /// <summary>
    /// Manages the playback and control of audio within the application.
    /// Provides methods for playing audio clips at specific positions or tracking transforms,
    /// while optimally managing resources through object pooling of audio sources.
    /// </summary>
    public class AudioManager : Singleton<AudioManager> {

        private readonly Dictionary<string, AudioClip> _clips = new();
        private readonly ObjectPool<AudioHandle> _pool;

        internal AudioMixer Mixer =>
            _mixer ??= Addressables.LoadAssetAsync<AudioMixer>("AudioMixer").WaitForCompletion();

        private AudioMixer _mixer;

        public AudioManager() {
            if (IsInitialized) return;
            _pool = new ObjectPool<AudioHandle>(OnPoolCreate, OnPoolGet, OnPoolRelease, OnPoolDestroy);
        }

        /// <summary>
        /// Change mixer group volume
        /// </summary>
        /// <param name="param">e.g MasterVolume, SfxVolume</param>
        /// <param name="value">Percentage</param>
        public void ChangeMixerVolume(string param, float value) {
            if (value <= 0) {
                Mixer.SetFloat(param, -80f);
                return;
            }

            Mixer.SetFloat(param, Mathf.Log10(value) * 20f);
        }

        public void Release(AudioHandle handle) {
            _pool.Release(handle);
        }

        private static AudioHandle OnPoolCreate() {
            var sourceObject = new GameObject("PooledAudioSource");
            sourceObject.AddComponent<AudioSource>();
            return sourceObject.AddComponent<AudioHandle>();
        }

        private static void OnPoolGet(AudioHandle handle) {
            handle.enabled = true;
            handle.source.enabled = true;
        }

        private static void OnPoolRelease(AudioHandle handle) {
            handle.source.Stop();
            handle.source.enabled = false;
            handle.enabled = false;
        }

        private static void OnPoolDestroy(AudioHandle handle) {
            Object.Destroy(handle.gameObject);
        }

        /// <summary>
        /// Plays an audio clip using the specified configuration, spatial properties, and optional position or tracking.
        /// The method either places the audio at a fixed position or follows a specific transform during playback.
        /// If a position is provided, the audio will play at the specified location in world space.
        /// Otherwise, if a transform is provided, the audio will track the transform's position.
        /// </summary>
        /// <param name="def">An <see cref="AudioDef"/> structure containing the audio asset configuration and playback properties.</param>
        /// <param name="spatialBlend">The spatial blend value where 0 is fully 2D and 1 is fully 3D.</param>
        /// <param name="position">The optional position in world space where the audio should play. If null, the method will track a transform if provided.</param>
        /// <param name="tracked">The optional transform to be tracked during playback. If null, the method will use the specified position, if provided.</param>
        public AudioHandle Play(AudioDef def, float spatialBlend, Vector3? position = null, Transform tracked = null) {
            var clip = def.AsClip();
            var trackedObject = _pool.Get();
            trackedObject.source.clip = clip;
            trackedObject.source.spatialBlend = spatialBlend;
            def.Apply(trackedObject.source);
            if (position.HasValue) {
                trackedObject.transform.position = position.Value;
                trackedObject.Play(position.Value);
                return trackedObject;
            }

            trackedObject.PlayTracked(tracked);
            return trackedObject;
        }

        internal AudioClip GetClip(string asset) {
            if (!_clips.ContainsKey(asset)) {
                _clips[asset] = Addressables.LoadAssetAsync<AudioClip>(asset).WaitForCompletion();
            }

            return _clips[asset];
        }

    }

    public class AudioHandle : MonoBehaviour {

        public bool IsPlaying => source.isPlaying;
        public bool HasEnded { get; private set; }

        [HideInInspector]
        public AudioSource source;

        [HideInInspector]
        public Transform tracked;

        public Action onEnd;

        private bool _waitingForStart;
        private Transform _transform;

        private void Awake() {
            _transform = transform;
            source = GetComponent<AudioSource>();
        }

        private void Update() {
            if (_waitingForStart && source.isPlaying) {
                _waitingForStart = false;
                return;
            }

            if (tracked) {
                _transform.position = tracked.position;
            }

            if (source.isPlaying || _waitingForStart) return;
            AudioManager.Instance.Release(this);
            if (HasEnded) return;
            HasEnded = true;
            onEnd?.Invoke();
        }


        public void Play(Vector3 position) {
            _transform.position = position;
            _waitingForStart = true;
            tracked = null;
            HasEnded = false;
            onEnd = null;
            source.Play();
        }

        public void PlayTracked(Transform tracked) {
            this.tracked = tracked;
            _transform.position = tracked.position;
            _waitingForStart = true;
            HasEnded = false;
            onEnd = null;
            source.Play();
        }

    }

    /// <summary>
    /// Represents a definition for grouping audio that shares common properties,
    /// such as an associated Audio Mixer Group for routing and effects processing.
    /// Provides functionality to retrieve the corresponding AudioMixerGroup based
    /// on a specified path in the Audio Mixer hierarchy.
    ///
    /// Example paths: Master/SFX, Master/Music, Master/Ambience
    /// </summary>
    public class AudioGroupDef {

        private readonly string _path;

        public AudioGroupDef(string path) {
            _path = path;
        }

        public AudioMixerGroup MixerGroup =>
            _mixerGroup ??= AudioManager.Instance.Mixer.FindMatchingGroups(_path).First();

        private AudioMixerGroup _mixerGroup;

    }

    public readonly struct AudioDef {

        public readonly string[] audioAsset;
        public readonly AudioGroupDef group;
        public readonly bool loop;
        public readonly float volume;
        public readonly float pitch;
        public readonly float minDistance;
        public readonly float maxDistance;

        public AudioDef(string[] audioAsset, AudioGroupDef group = null, bool loop = false, float volume = 1f,
            float pitch = 1f, float minDistance = 1f,
            float maxDistance = 15f) {
            this.audioAsset = audioAsset;
            this.group = group;
            this.loop = loop;
            this.volume = volume;
            this.pitch = pitch;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
        }

        public AudioDef With(bool? loop = null, float? volume = null, float? pitch = null,
            float? minDistance = null, float? maxDistance = null) {
            return new AudioDef(audioAsset, group, loop ?? this.loop, volume ?? this.volume, pitch ?? this.pitch,
                minDistance ?? this.minDistance, maxDistance ?? this.maxDistance);
        }

        internal void Apply(AudioSource source) {
            if (group != null) source.outputAudioMixerGroup = group.MixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.loop = loop;
        }

        /// <summary>
        /// Plays an audio clip with default parameters or previously configured properties.
        /// The playback utilizes the settings defined in the <see cref="AudioDef"/> such as volume, pitch, and spatialization.
        /// If positional or transform tracking parameters are needed, consider using `PlayAt` or `PlayTracked` methods.
        /// </summary>
        public AudioHandle Play() {
            return PlayAt(Vector3.zero, 0f);
        }

        /// <summary>
        /// Retrieves the audio clip associated with this <see cref="AudioDef"/> using the audio asset name.
        /// The clip can be used for custom playback or manipulation outside the <see cref="AudioManager"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AudioClip"/> instance corresponding to the audio asset defined in this <see cref="AudioDef"/>.
        /// Returns null if the audio asset is not found in the <see cref="AudioManager"/>.
        /// </returns>
        public AudioClip AsClip() {
            return AudioManager.Instance.GetClip(GetRandomAudioAsset());
        }

        public string GetRandomAudioAsset() {
            return audioAsset.Length == 1 ? audioAsset[0] : audioAsset[Random.Range(0, audioAsset.Length)];
        }

        /// <summary>
        /// Plays an audio clip at the specified position in world space with the specified spatial blend.
        /// </summary>
        /// <param name="position">The position in world space where the audio should be played.</param>
        /// <param name="spatialBlend">The spatial blend value where 0 is fully 2D and 1 is fully 3D. Defaults to 1f.</param>
        public AudioHandle PlayAt(Vector3 position, float spatialBlend = 1f) {
            return AudioManager.Instance.Play(this, spatialBlend, position);
        }

        /// <summary>
        /// Plays an audio clip while tracking the position of a specified transform during playback.
        /// The spatial properties of the audio clip can be customized using the spatial blend parameter.
        /// </summary>
        /// <param name="tracked">The transform to be tracked during audio playback. The audio will follow the position of this transform in real-time.</param>
        /// <param name="spatialBlend">The spatial blend value where 0 is fully 2D and 1 is fully 3D.</param>
        public AudioHandle PlayTracked(Transform tracked, float spatialBlend = 1f) {
            return AudioManager.Instance.Play(this, spatialBlend, null, tracked);
        }

        /// <summary>
        /// Plays an audio clip while tracking the position of a specified object during playback.
        /// The object's transform will determine the audio position in real-time. The spatial blend can be adjusted to control the 2D/3D effect.
        /// </summary>
        /// <param name="tracked">The object whose transform will be tracked during audio playback. Must be a <see cref="GameObject"/> or <see cref="Component"/>.</param>
        /// <param name="spatialBlend">The spatial blend value where 0 is fully 2D and 1 is fully 3D.</param>
        /// <exception cref="ArgumentException">Thrown if the tracked object is not of a supported type.</exception>
        public AudioHandle PlayTracked(Object tracked, float spatialBlend = 1f) {
            return tracked switch {
                GameObject go => PlayTracked(go.transform, spatialBlend),
                Component component => PlayTracked(component.transform, spatialBlend),
                _ => throw new ArgumentException("Invalid tracked object type.")
            };
        }

    }
}