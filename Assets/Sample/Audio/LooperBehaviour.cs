using Cysharp.Threading.Tasks;
using Sample.Audio;
using Sirenix.OdinInspector;
using Spookline.SPC.Audio;
using Spookline.SPC.Ext;

public class LooperBehaviour : SpookBehaviour {

    public LoopingAudioJob loopingJob;
    public float spatialBlend = 1f;
    public float crossFadeDuration = 0.2f;
    
    [Button]
    public void Setup() {
        Stop();
        
        loopingJob = new LoopingAudioJob(AudioKeys.Drone.Builder(), transform, crossFadeDuration, 2, spatialBlend);
        loopingJob.Setup().Forget();
    }

    [Button]
    public void Stop() {
        loopingJob?.Stop();
    }

    [Button]
    public void Continue() {
        loopingJob?.Start();
    }

    [Button]
    public void Dispose() {
        loopingJob?.Dispose();
    }

}