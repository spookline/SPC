using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spookline.SPC.Audio {
    /// <summary>
    /// Manages audio-related functionalities such as playing, stopping, and caching audio clips
    /// in the application. Facilitates the use of audio across multiple objects and scenes.
    /// </summary>
    public class AudioManager : Singleton<AudioManager> {

        private readonly Dictionary<string, AudioClip> _clips = new();
        private readonly ObjectPool<AudioTrackedObject> _pool;
        
        public AudioManager() {
            if (IsInitialized) return;
            _pool = new ObjectPool<AudioTrackedObject>(OnPoolCreate, OnPoolGet, OnPoolRelease, OnPoolDestroy);
        }

        public void Release(AudioTrackedObject trackedObject) {
            _pool.Release(trackedObject);
        }

        private static AudioTrackedObject OnPoolCreate()  {
            var sourceObject = new GameObject("PooledAudioSource");
            sourceObject.AddComponent<AudioSource>();
            return sourceObject.AddComponent<AudioTrackedObject>();
        }

        private static void OnPoolGet(AudioTrackedObject trackedObject) {
            trackedObject.enabled = true;
            trackedObject.source.enabled = true;
        }

        private static void OnPoolRelease(AudioTrackedObject trackedObject) {
            trackedObject.source.Stop();
            trackedObject.source.enabled = false;
            trackedObject.enabled = false;
        }
        
        private static void OnPoolDestroy(AudioTrackedObject trackedObject) {
            Object.Destroy(trackedObject.gameObject);
        }

        public void PlayAtTarget(AudioDefinition definition, GameObject target) {
            
        }
        
        public void Play(AudioDefinition definition) {
            Play(definition, Vector3.zero);
        }
        
        public void Play(AudioDefinition definition, Vector3 position) {
            var clip = GetClip(definition.audioAsset);
            if(clip == null) return;
            var trackedObject = _pool.Get();
            trackedObject.source.clip = clip;
            definition.Apply(trackedObject.source);
            trackedObject.Play(position);
        }

        private AudioClip GetClip(string asset) {
            if (!_clips.ContainsKey(asset)) {
                _clips[asset] = Addressables.LoadAssetAsync<AudioClip>(asset).WaitForCompletion();
            }
            return _clips[asset];
        }

    }

    public class AudioTrackedObject : MonoBehaviour {

        [HideInInspector]
        public AudioSource source;
        
        private bool _started;

        private void Awake() {
            source = GetComponent<AudioSource>();
        }

        private void Update() {
            if (_started && source.isPlaying) {
                _started = false;
                return;
            }
            if (!source.isPlaying && !_started) {
                AudioManager.Instance.Release(this);
            }
        }



        public void Play(Vector3 position) {
            transform.position = position;
            _started = true;
            source.Play();
        }
        

    }

    public class AudioDefinition {

        public readonly string audioAsset;
        public bool loop = false;
        public float volume = 1f;
        public float pitch = 1f;
        public float spatialBlend = 0f;
        public float minDistance = 0f;
        public float maxDistance = 15f;

        public AudioDefinition(string audioAsset) {
            this.audioAsset = audioAsset;
        }
        public void Apply(AudioSource source) {
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = spatialBlend;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.loop = loop;
        }
        

    }

    public class AudioPlayer {

        public Dictionary<AudioDefinition, AudioSource> sources = new();

    }
}