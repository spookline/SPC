using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Spookline.SPC.Audio {
    /// <summary>
    /// Manages audio-related functionalities such as playing, stopping, and caching audio clips
    /// in the application. Facilitates the use of audio across multiple objects and scenes.
    /// </summary>
    public class AudioManager : Singleton<AudioManager> {

        private readonly Dictionary<string, AudioClip> _clips = new();
        private readonly Dictionary<GameObject, AudioPlayer> _players = new();

        private GameObject FallbackTarget => _fallbackTarget ??= new GameObject("AudioMainContainer");
        private GameObject _fallbackTarget;

        public AudioManager() {
            if (IsInitialized) return;
            // Cleanup
            SceneManager.sceneUnloaded += _ => {
                var keys = new List<GameObject>(_players.Keys);
                foreach (var key in keys.Where(key => key == null)) {
                    _players.Remove(key);
                }
            };
        }

        /// <summary>
        /// Plays an audio clip based on the provided audio definition, optional target GameObject, and delay time.
        /// </summary>
        /// <param name="definition">The audio definition specifying the audio clip to play, along with its playback properties such as loop, volume, and pitch.</param>
        /// <param name="targetObject">The target GameObject on which the audio clip should be played. Defaults to a fallback GameObject if not specified.</param>
        /// <param name="delay">The time in seconds to delay the playback of the audio clip. Defaults to 0.0f for immediate playback.</param>
        public void Play(AudioDefinition definition, GameObject targetObject = null, float delay = 0.0f) {
            var clip = GetClip(definition.audioAsset);
            if (clip == null) return;
            var target = targetObject ?? FallbackTarget;
            var source = GetOrCreateSource(definition, target);
            if (delay > 0.0f) {
                source.Play();
            } else {
                source.PlayDelayed(delay);
            }
        }

        /// <summary>
        /// Stops the playback of an audio clip based on the provided audio definition and optional target GameObject.
        /// </summary>
        /// <param name="definition">The audio definition identifying the specific audio clip to stop.</param>
        /// <param name="targetObject">The target GameObject from which the audio clip should be stopped. Defaults to a fallback GameObject if not provided.</param>
        public void Stop(AudioDefinition definition, GameObject targetObject = null) {
            var target = targetObject ?? FallbackTarget;
            if (!_players.TryGetValue(target, out var player)) return;
            if (player.sources.TryGetValue(definition, out var source)) {
                source.Stop();
            }
        }

        private AudioSource CreateSource(AudioDefinition definition, GameObject target) {
            var audioSource = target.AddComponent<AudioSource>();
            audioSource.clip = GetClip(definition.audioAsset);
            audioSource.volume = definition.volume;
            audioSource.pitch = definition.pitch;
            audioSource.spatialBlend = definition.spatialBlend;
            audioSource.minDistance = definition.minDistance;
            audioSource.maxDistance = definition.maxDistance;
            audioSource.loop = definition.loop;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            return audioSource;
        }

        private AudioSource GetOrCreateSource(AudioDefinition definition, GameObject target) {
            if (!_players.ContainsKey(target)) {
                _players[target] = new AudioPlayer();
            }

            var player = _players[target];
            if (!player.sources.ContainsKey(definition)) {
                player.sources[definition] = CreateSource(definition, target);
            }

            return player.sources[definition];
        }

        private AudioClip GetClip(string asset) {
            if (!_clips.ContainsKey(asset)) {
                _clips[asset] =
                    Addressables.LoadAssetAsync<AudioClip>(asset).WaitForCompletion();
            }

            return _clips[asset];
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

    }

    public class AudioPlayer {

        public Dictionary<AudioDefinition, AudioSource> sources = new();

    }
}