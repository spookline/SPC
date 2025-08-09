using Spookline.SPC.Audio;
using Spookline.SPC.Registry;

namespace Sample.Audio {
    public static class AudioDefs {

        public static readonly AudioGroupDef Sfx = new("Master/SFX");

        public static AudioDef Fart = new(new[] { "fart" }, Sfx);
        public static AudioDef Ambience = new(new[] { "Ambience" }, Sfx);

    }

    public enum AudioKeys {

        Drone,
        Fart,

    }

    public static class AudioKeysExtension {

        public static AudioJob Builder(this AudioKeys key) => SpookAudioRegistry.EnumJobInterop(key);

    }
}