using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

namespace Spookline.SPC {
    public class Globals : MonoBehaviour {

        public static Globals Instance { get; private set; }

        public List<IModule> modules;
        public Dictionary<Type, ModuleInstance> ModulesByType { get; private set; } = new();

        public void ModdingEntrypoint() { }

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
        }

        private void OnDestroy() {
            foreach (var module in modules) {
                module.Unload();
            }

            Instance = null;
        }

        // Update is called once per frame
        void Update() { }
        
    }
}