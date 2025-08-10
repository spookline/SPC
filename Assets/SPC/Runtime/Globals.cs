using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

namespace Spookline.SPC {
    public class Globals : MonoBehaviour {

        [SerializeField]
        public List<Module> modules;

        public static Globals Instance { get; private set; }
        public Dictionary<Type, ModuleInstance> ModulesByType { get; } = new();

        public bool Started { get; private set; }

        // Start is called before the first frame update
        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ModdingEntrypoint();
            foreach (var module in modules) {
                var instance = new ModuleInstance(module);
                ModulesByType[module.GetTypeDelegate()] = instance;
                module.Load();
            }

            AsyncInitFlow().Forget();
        }

        // Update is called once per frame
        private void Update() { }

        private void OnDestroy() {
            foreach (var module in modules) module.Unload();

            Instance = null;
            Started = false;
        }

        public static UniTask UntilStarted() {
            return UniTask.WaitUntil(() => Instance && Instance.Started);
        }

        private async UniTask AsyncInitFlow() {
            await new GlobalStartEvt().RaiseAsync();
            Started = true;
        }

        public void ModdingEntrypoint() { }

    }

    public class GlobalStartEvt : AsyncChainEvt<GlobalStartEvt> { }
}