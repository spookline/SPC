using Spookline.SPC.Audio;
using Spookline.SPC.Registry;

namespace Sample.Audio {

    public enum AudioKeys {

        Drone,
        Fart,

    }

    public static class AudioKeysExtension {

        public static AudioJob Builder(this AudioKeys key) => SpookAudioRegistry.EnumJobInterop(key);

    }
}