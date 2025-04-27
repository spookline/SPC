using Spookline.SPC.Audio;

namespace Sample.Audio {
    public static class AudioDefs {
        
        public static readonly AudioGroupDef Sfx = new("Master/SFX");

        public static AudioDef Fart = new(new [] {"fart"}, Sfx);

    }
}