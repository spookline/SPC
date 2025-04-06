using UnityEngine;

namespace Spookline.SPC.Audio {

    public class AudioCache {

        

    }
    
    public struct Audio {

        public readonly string audioAsset;
        public bool loop;
        public float volume;
        public float pitch;
        public float spatialBlend;
        public float minDistance;
        public float maxDistance;

        public Vector3 position;

        public Audio(string audioAsset) {
            this.audioAsset = audioAsset;
            loop = false;
            volume = 1f;
            pitch = 1f;
            spatialBlend = 0f;
            minDistance = 0f;
            maxDistance = 15f;
            position = Vector3.zero;
        }

        public void Play() {
            
        }
        
    }

}