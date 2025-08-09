using System;
using Spookline.SPC.Registry;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Spookline.SPC.Audio {
    public class SpookAudioRegistry : ObjectRegistry<SpookAudioRegistry, AudioDefinition> {

        public override string AddressableLabel => "audio";

        public static AudioDefinition EnumInterop(Enum enumValue) => Instance.GetByEnum(enumValue);
        public static AudioJob EnumJobInterop(Enum enumValue) {
            var def = EnumInterop(enumValue);
            if (!def) throw new ArgumentException($"No audio definition found for enum value {enumValue}");
            return SpookAudioModule.Instance.CreateJob(def);
        }

    }

    public static class SpookAudioRegistryExtensions {

        public static AudioDefinition FromRegistry(this AssetReferenceT<AudioDefinition> reference) =>
            SpookAudioRegistry.Instance.GetByGuid(reference.AssetGUID);


        public static AudioJob Job(this AudioDefinition def) =>
            SpookAudioModule.Instance.CreateJob(def);
        
    }
}