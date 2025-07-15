using Sample.Audio;
using Spookline.SPC.Audio;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Play();
        }
    }

    private static void Play() {
        var handle = AudioDefs.Fart.Play();
        handle.onEnd += Play;
    }

}