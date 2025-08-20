using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spookline.SPC.Ext;
using Spookline.SPC.Save;
using UnityEngine;

namespace Sample.Scripts {
    public class SaveSampleBehaviour : SpookBehaviour {

        private void Awake() {
            On<GameSaveEvt>().ChainDo(OnSaveGame);
            On<GameLoadEvt>().ChainDo(OnLoadGame);
        }

        [Button]
        public void Save() {
            Inner().Forget();
            return;

            async UniTask Inner() {
                var save = await SpookSaveModule.Instance.SaveGame();
                SpookSaveModule.Instance.SaveToFile(save);
            }
        }

        [Button]
        public void ListAllSaves() {
            var saves = SpookSaveModule.Instance.GetSaveFiles();
            Debug.Log($"Found {saves.Count} saves:");
            foreach (var save in saves) {
                Debug.Log($"{save.Name} - {save.Time}");
            }
        }

        [Button]
        public void LoadLatestSave() {
            Inner().Forget();
            return;

            async UniTask Inner() {
                var saves = SpookSaveModule.Instance.GetSaveFiles();
                var saveFile = saves.FirstOrDefault();
                if (saveFile == null) {
                    Debug.LogWarning("No saves found.");
                    return;
                }

                Debug.Log($"Loading save: {saveFile.Name}");
                var saveGame = SpookSaveModule.Instance.LoadSaveFile(saveFile);
                await SpookSaveModule.Instance.TriggerLoad(saveGame);
            }
        }

        [Button]
        public void DumpLatestSave() {
            var saves = SpookSaveModule.Instance.GetSaveFiles();
            var saveFile = saves.FirstOrDefault();
            if (saveFile == null) {
                Debug.LogWarning("No saves found.");
                return;
            }

            Debug.Log($"Loading save: {saveFile.Name}");
            var saveGame = SpookSaveModule.Instance.LoadSaveFile(saveFile);
            var jsonDump = SpookSaveModule.Instance.JsonDumpSaveGame(saveGame);
            Debug.Log("Save Game JSON Dump:\n" + jsonDump);
        }

        [Button]
        public void DeleteLastSave() {
            var saves = SpookSaveModule.Instance.GetSaveFiles();
            var saveFile = saves.LastOrDefault();
            if (saveFile == null) {
                Debug.LogWarning("No saves found to delete.");
                return;
            }

            Debug.Log($"Deleting save: {saveFile.Name}");
            SpookSaveModule.Instance.DeleteSaveFile(saveFile);
        }

        private void OnLoadGame(GameLoadEvt arg) {
            if (arg.TryReadData<float>("sample", out var time)) {
                Debug.Log($"Loaded time data: {time}");
            }
        }

        private void OnSaveGame(GameSaveEvt arg) {
            arg.WriteData("sample", Time.time);
        }

    }
}