using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Spookline.SPC.Audio {
    public class AudioHandle : MonoBehaviour {

        [HideInInspector]
        public AudioSource source;

        [HideInInspector]
        public Transform tracked;
        private Transform _transform;

        public Action onEnd;
        public Action onContinuation;

        private AudioHandleState _state;
        private AudioJobReference _jobReference;

        public AudioHandleState State {
            get => _state;
        }

        public bool IsPlaying => source.isPlaying;
        public bool HasEnded => _state is AudioHandleState.Ended or AudioHandleState.Released;
        public bool KeepAlive { get; set; } = false;

        public bool IsReleased => _state == AudioHandleState.Released || _jobReference == null;

        private float _fadeOutStart;
        private float _fadeOutEnd;
        private bool _fadeOut;
        private bool _startedFadeOut;

        private float _fadeInStart;
        private float _fadeInEnd;
        private bool _fadeIn;
        private bool _startedFadeIn;
        private bool _hasCalledContinuation;

        private float _targetVolume;

        private void Awake() {
            _transform = transform;
            source = GetComponent<AudioSource>();
        }

        public void ApplyChanges(AudioJobReference reference) {
            _targetVolume = source.volume;
            _jobReference = reference;
        }

        public void ReleaseCallback() {
            _jobReference = null;
            _state = AudioHandleState.Released;
            _fadeOut = false;
            _fadeIn = false;
            _startedFadeIn = false;
            _startedFadeOut = false;
            onEnd = null;
            onContinuation = null;
            source.clip = null;
            Debug.Log("Audio handle released");
        }


        public bool IsOwnedBy(AudioJobReference reference) {
            return _jobReference != null && _jobReference.Equals(reference);
        }

        public void ImmediateFadeOut(float duration) {
            var time = source.time;
            var endTime = Mathf.Min(time + duration, source.clip.length);
            _fadeOutStart = time;
            _fadeOutEnd = endTime;
            _fadeOut = true;
        }

        public void SetFadeIn(float duration) {
            _fadeInStart = 0f;
            _fadeInEnd = duration;
            _fadeIn = true;
        }

        public void SetFadeOut(float duration) {
            _fadeOutStart = source.clip.length - duration;
            _fadeOutEnd = source.clip.length;
            _fadeOut = true;
        }


        private void Update() {
            if (_state == AudioHandleState.Released) return;

            if (tracked) _transform.position = tracked.position;
            switch (_state) {
                case AudioHandleState.Ended:
                case AudioHandleState.Idle:
                    break;
                case AudioHandleState.Starting:
                    if (_jobReference.state == AudioJobReferenceState.Killed) {
                        _state = AudioHandleState.Ended;
                        break;
                    }
                    
                    if (source.time > 0 && source.isPlaying) {
                        _state = AudioHandleState.Playing;
                    }

                    if (_fadeIn) {
                        source.volume = 0;
                    }

                    break;
                case AudioHandleState.Playing:
                    if (!source.isPlaying) {
                        EndNow();
                        break;
                    }

                    var time = source.time;
                    float controlVolume = -1;
                    if (_fadeIn) {
                        if (time >= _fadeInStart && time <= _fadeInEnd) {
                            if (!_startedFadeIn) {
                                _startedFadeIn = true;
                                controlVolume = 0;
                            } else {
                                var t = (time - _fadeInStart) / (_fadeInEnd - _fadeInStart);
                                controlVolume = _targetVolume * t;
                            }
                        } else if (time >= _fadeInStart && time < _fadeInEnd) {
                            controlVolume = _targetVolume;
                            _fadeIn = false;
                            _startedFadeIn = false;
                        }
                    }

                    if (_fadeOut) {
                        if (time >= _fadeOutStart && time <= _fadeOutEnd) {
                            if (!_startedFadeOut) {
                                _startedFadeOut = true;
                                controlVolume = _targetVolume;
                                CallContinuation();
                                Debug.Log("Audio fade out begun");
                            } else {
                                var t = (time - _fadeOutStart) / (_fadeOutEnd - _fadeOutStart);
                                controlVolume = _targetVolume * (1f - t);
                            }
                        } else if (time >= _fadeOutStart && time < _fadeOutEnd) {
                            Debug.Log($"Audio fade out end at {time} with end {_fadeOutStart}-{_fadeOutEnd}");
                            controlVolume = 0f;
                            EndNow();
                        }
                    }

                    if (controlVolume >= 0) {
                        var curvedVolume = Mathf.Log10(controlVolume * 100f + 1f) / 2f;
                        source.volume = controlVolume;
                    }

                    break;
            }
        }

        public void EndNow() {
            if (HasEnded) return;
            Debug.Log("Audio clip has ended");

            source.Stop();
            _state = AudioHandleState.Ended;
            _startedFadeIn = false;
            _startedFadeOut = false;

            onEnd?.Invoke();
            CallContinuation();
            if (KeepAlive) {
                _state = AudioHandleState.Idle;
            } else if (!IsReleased) AudioManager.Instance.Release(this);
        }

        public void Kill() {
            if (!IsReleased) AudioManager.Instance.Release(this);
        }

        private void CallContinuation() {
            if (_hasCalledContinuation) return;
            _hasCalledContinuation = true;
            onContinuation?.Invoke();
        }


        private void OnDestroy() {
            if (!IsReleased) AudioManager.Instance.Release(this);
        }


        public void Play(Vector3 position) {
            _transform.position = position;
            tracked = null;
            PrepareStart();
        }

        public void PlayTracked(Transform tracked) {
            this.tracked = tracked;
            _transform.position = tracked.position;
            PrepareStart();
        }

        private void PrepareStart() {
            _startedFadeIn = false;
            _startedFadeOut = false;
            _hasCalledContinuation = false;
            _state = AudioHandleState.Starting;
            if (_fadeIn) source.volume = 0f;
            source.Play();
        }

    }

    public enum AudioHandleState {

        Idle,
        Starting,
        Playing,
        Ended,
        Released

    }

    public class AudioJobReference : IDisposable {

        public AudioHandle handle;
        public AudioJob job;
        internal AudioJobReferenceState state = AudioJobReferenceState.Uninitialized;

        public bool IsValid => handle && handle.IsOwnedBy(this) || state >= 0;
        public bool IsPlaying => IsValid && handle.IsPlaying;
        public bool IsPending => state == AudioJobReferenceState.Pending;

        /// <summary>
        /// Stops a job if it is currently playing.
        /// </summary>
        public void Stop() {
            if (IsValid && !handle.HasEnded) {
                handle.EndNow();
            }
        }
        

        /// <summary>
        /// Fully disposes this job reference even if it is still pending.
        /// </summary>
        public void Dispose() {
            if (IsValid) {
                Stop();
                state = AudioJobReferenceState.Killed;
                if (!handle.IsReleased) AudioManager.Instance.Release(handle);
            }
        }

    }

    public enum AudioJobReferenceState : int {
        Killed = -2,
        Uninitialized = -1,
        Pending = 0,
        Ready = 1,

    }

    public class LoopingAudioJob : IDisposable {

        public readonly AudioJob definition;
        public readonly Transform tracked;
        public readonly int cycleCount;
        public readonly float spatialBlend;
        public readonly float crossfadeDuration;

        public LoopingAudioJob(
            AudioJob definition,
            Transform tracked = null,
            float crossfadeDuration = 0.25f,
            int cycleCount = 2,
            float spatialBlend = 1f
        ) {
            this.definition = definition;
            this.tracked = tracked;
            this.crossfadeDuration = crossfadeDuration;
            this.cycleCount = Mathf.Max(2, cycleCount);
            this.spatialBlend = spatialBlend;
        }

        public bool IsRunning => _active && _cycle != null;

        private int _currentIndex;
        private bool _active = false;
        private AudioJobReference[] _cycle;

        public async UniTask Setup(bool autostart = false) {
            if (_cycle != null) {
                Debug.LogWarning("LoopingAudioJob is already set up, skipping setup.");
                return;
            }

            _cycle = new AudioJobReference[cycleCount];
            for (var i = 0; i < cycleCount; i++) {
                var finalizedIndex = i;
                var reference = await definition.Unstarted();
                reference.handle.onContinuation += () => OnFadeOutBegin(finalizedIndex);
                reference.handle.KeepAlive = true;
                _cycle[i] = reference;
            }

            _currentIndex = 0;
            if (autostart) Start();
        }

        public void Start() {
            if (_cycle == null) {
                Debug.LogError("LoopingAudioJob is not set up, call Setup() first.");
                return;
            }

            _active = true;
            StartAt(0);
        }

        public void Stop() {
            if (_cycle == null) return;
            _active = false;
            foreach (var reference in _cycle) {
                reference.Stop();
            }
        }

        public void Dispose() {
            Stop();
            if (_cycle == null) return;
            foreach (var reference in _cycle) {
                reference.Dispose();
            }

            _cycle = null;
        }

        private void OnFadeOutBegin(int index) {
            if (!_active) return;
            var nextIndex = (index + 1) % cycleCount;
            StartAt(nextIndex);
            Debug.Log($"LoopingAudioJob: Fading out index {index}, starting at index {nextIndex}");
        }

        private void StartAt(int index) {
            if (!_active) return;
            var reference = _cycle[index];
            var handle = reference.handle;
            if (crossfadeDuration > 0) {
                handle.SetFadeIn(crossfadeDuration);
                handle.SetFadeOut(crossfadeDuration);
            }

            if (tracked) {
                handle.source.spatialBlend = spatialBlend;
                handle.PlayTracked(tracked);
            } else {
                handle.source.spatialBlend = 0f;
                handle.Play(Vector3.zero);
            }

            _currentIndex = index;
        }

    }
}