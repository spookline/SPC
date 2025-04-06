using Sample.Audio;
using UnityEngine;

public class AudioTest : MonoBehaviour {

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            AudioDefs.Fart.Play();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            AudioDefs.Fart.With(volume: 0.1f).Play();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            var go = GameObject.Find("Tracked");
            AudioDefs.Fart.PlayAt(go.transform.position);
        }
    }

}