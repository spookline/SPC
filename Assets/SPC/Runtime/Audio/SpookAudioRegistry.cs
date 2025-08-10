using System;
using Spookline.SPC.Registry;
using UnityEngine.AddressableAssets;

namespace Spookline.SPC.Audio {
    public class SpookAudioRegistry : ObjectRegistry<SpookAudioRegistry, AudioDefinition> {

        public override string AddressableLabel => "audio";

        public static AudioDefinition EnumInterop(Enum enumValue) {
            return Instance.GetByEnum(enumValue);
        }

        public static AudioJob EnumJobInterop(Enum enumValue) {
            var def = EnumInterop(enumValue);
            if (!def) throw new ArgumentException($"No audio definition found for enum value {enumValue}");
            return SpookAudioModule.Instance.CreateJob(def);
        }

    }

    public static class SpookAudioRegistryExtensions {

        public static AudioDefinition FromRegistry(this AssetReferenceT<AudioDefinition> reference) {
            return SpookAudioRegistry.Instance.GetByGuid(reference.AssetGUID);
        }


        public static AudioJob Job(this AudioDefinition def) {
            return SpookAudioModule.Instance.CreateJob(def);
        }

    }
}