using Cysharp.Threading.Tasks;
using Spookline.SPC.Ext;
using UnityEngine;

namespace Spookline.SPC.Audio {
    [CreateAssetMenu(fileName = "SpookAudioModule", menuName = "Modules/SpookAudioModule")]
    public class SpookAudioModule : OdinModule<SpookAudioModule> {

        public AudioJob CreateJob(AudioDefinition definition) {
            return definition.provider.CreateJob(definition);
        }

        private async UniTask Prepare(AudioDefinition definition) {
            if (!definition.provider.IsLoaded) await definition.provider.Load();
        }
        
        private async UniTask<AudioHandle> Lease(AudioJob job) {
            await Prepare(job.definition);
            var handle = AudioManager.Instance.Lease();
            job.definition.provider.Apply(handle, job);
            return handle;
        }

        public async UniTask Play(AudioJob job) {
            var handle = await Lease(job);
            handle.source.spatialBlend = 0f;
            handle.Play(Vector3.zero);
        }
        
        public async UniTask Play(AudioJob job, Vector3 position) {
            var handle = await Lease(job);
            handle.source.spatialBlend = 1f;
            handle.Play(position);
        }
        
        public async UniTask Play(AudioJob job, Transform transform) {
            var handle = await Lease(job);
            handle.source.spatialBlend = 1f;
            handle.PlayTracked(transform);
        }
        
        public async UniTask<AudioClip> GetClip(AudioJob job) {
            await Prepare(job.definition);
            return job.definition.provider.GetClip(job);
        }
    }
}