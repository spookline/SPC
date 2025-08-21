using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spookline.SPC.Ext;
using UnityEngine;
using UnityEngine.Audio;

namespace Spookline.SPC.Audio {
    [CreateAssetMenu(fileName = "SpookAudioModule", menuName = "Modules/SpookAudioModule")]
    public class SpookAudioModule : OdinModule<SpookAudioModule> {
        public static UniTask Ready => UniTask.WaitUntil(() => HasInstance && Instance.ready);
        
        public AudioMixer mixer;
        public int audioCacheSize = 10;
        public bool ready = false;
        
        [TypeDrawerSettings(BaseType = typeof(Enum), Filter = TypeInclusionFilter.IncludeConcreteTypes)]
        public Type lookupEnum;

        public override void Load() {
            base.Load();
            ready = false;
            On<GlobalStartEvt>().ChainDo(OnGlobalStart, priority: -100);
        }

        public void ClearAudioPool() {
            AudioManager.Instance.ClearAudioPool();
        }

        private async UniTask OnGlobalStart(GlobalStartEvt arg) {
            await SpookAudioRegistry.Instance.Load(lookupEnum);
            AudioManager.Instance.Initialize(audioCacheSize);
            ready = true;
        }

        public AudioJob CreateJob(AudioDefinition definition) {
            return definition.provider.CreateJob(definition);
        }

        private async UniTask Prepare(AudioDefinition definition) {
            if (!definition.provider.IsLoaded) await definition.provider.Load();
        }

        private async UniTask Lease(AudioJobReference reference) {
            var job = reference.job;
            await Prepare(job.definition);
            var handle = AudioManager.Instance.Lease();
            job.definition.provider.Apply(handle, job);
            reference.handle = handle;
            handle.ApplyChanges(reference);
        }

        public async UniTask Unstarted(AudioJobReference reference) {
            await Lease(reference);
            reference.state = AudioJobReferenceState.Ready;
        }

        public async UniTask Play(AudioJobReference reference) {
            await Lease(reference);
            var handle = reference.handle;
            handle.source.spatialBlend = 0f;
            if (reference.state == AudioJobReferenceState.Killed) return;
            reference.state = AudioJobReferenceState.Ready;
            handle.Play(Vector3.zero);
        }

        public async UniTask Play(AudioJobReference reference, Vector3 position) {
            await Lease(reference);
            var handle = reference.handle;
            handle.source.spatialBlend = 1f;
            if (reference.state == AudioJobReferenceState.Killed) return;
            reference.state = AudioJobReferenceState.Ready;
            handle.Play(position);
        }

        public async UniTask Play(AudioJobReference reference, Transform transform) {
            await Lease(reference);
            var handle = reference.handle;
            handle.source.spatialBlend = 1f;
            if (reference.state == AudioJobReferenceState.Killed) return;
            reference.state = AudioJobReferenceState.Ready;
            handle.PlayTracked(transform);
        }

        public async UniTask<AudioClip> GetClip(AudioJob job) {
            await Prepare(job.definition);
            return job.definition.provider.GetClip(job);
        }

    }
}