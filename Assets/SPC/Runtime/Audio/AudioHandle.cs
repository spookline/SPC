using System;
using UnityEngine;

namespace Spookline.SPC.Audio {
    public class AudioHandle : MonoBehaviour {

        [HideInInspector]
        public AudioSource source;

        [HideInInspector]
        public Transform tracked;
        private Transform _transform;

        private bool _waitingForStart;

        public Action onEnd;

        public bool IsPlaying => source.isPlaying;
        public bool HasEnded { get; private set; }

        private void Awake() {
            _transform = transform;
            source = GetComponent<AudioSource>();
        }

        private void OnDestroy() {
            AudioManager.Instance.Release(this);
        }

        private void Update() {
            if (_waitingForStart && source.time > 0) {
                _waitingForStart = false;
                return;
            }

            if (tracked) _transform.position = tracked.position;

            if (source.time > 0 || _waitingForStart) return;
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
}