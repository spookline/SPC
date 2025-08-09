using Sample.Audio;
using Sirenix.OdinInspector;
using Spookline.SPC.Audio;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    public Transform left;
    public Transform right;

    [Button]
    public void PlayLeft() {
        AudioKeys.Fart.Builder().PlayTracked(left);
    }

    [Button]
    public void PlayRight() {
        AudioKeys.Fart.Builder().PlayTracked(right);
    }

    [Button]
    public void Drone() {
        AudioKeys.Drone.Builder().Play();
    }
    

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            AudioKeys.Fart.Builder().Play();
        }
    }

    private static void Play() {
        var handle = AudioDefs.Fart.Play();
        handle.onEnd += Play;
    }

}