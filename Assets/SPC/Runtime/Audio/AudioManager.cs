using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Spookline.SPC.Ext;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Spookline.SPC.Audio {
    /// <summary>
    ///     Manages the playback and control of audio within the application.
    ///     Provides methods for playing audio clips at specific positions or tracking transforms,
    ///     while optimally managing resources through object pooling of audio sources.
    /// </summary>
    public class AudioManager : Singleton<AudioManager> {

        private readonly Dictionary<string, AudioClip> _clips = new();
        private ObjectPool<AudioHandle> _pool;

        private AudioMixer _mixer;

        public AudioManager() {
            if (IsInitialized) return;
        }

        internal void Initialize(int cacheSize) {
            _pool?.Dispose();
            _pool = new ObjectPool<AudioHandle>(OnPoolCreate, OnPoolGet, OnPoolRelease, OnPoolDestroy, maxSize: cacheSize);
        }

        internal void ClearAudioPool() {
            _pool.Clear();
        }
        
        public AudioMixer Mixer => _mixer ??= SpookAudioModule.Instance.mixer;
        
        /// <summary>
        ///     Generates a range of audio paths based on a prefix and an amount.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string[] GenerateRangedPaths(string prefix, int amount) {
            var paths = new string[amount];
            for (var i = 0; i < amount; i++) paths[i] = $"{prefix}_{i + 1}";
            return paths;
        }

        /// <summary>
        ///     Change mixer group volume
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

        internal AudioClip GetClip(string asset) {
            if (!_clips.ContainsKey(asset))
                _clips[asset] = Addressables.LoadAssetAsync<AudioClip>(asset).WaitForCompletion();

            return _clips[asset];
        }

        internal AudioHandle Lease() {
            return _pool.Get();
        }

        internal void Release(AudioHandle handle) {
            handle.ReleaseCallback();
            _pool.Release(handle);
        }

        #region Pool Callbacks

        private static AudioHandle OnPoolCreate() {
            var sourceObject = new GameObject("PooledAudioSource");
            sourceObject.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(sourceObject);
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

        #endregion

    }
}