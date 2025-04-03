using System;
using System.Collections.Generic;
using System.Linq;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spookline.SPC {
    public class Globals : MonoBehaviour {

        public static Globals Instance { get; private set; }

        public List<Module> modules;
        public Dictionary<Type, ModuleInstance> ModulesByType { get; private set; } = new();
        public EventManager EventManager { get; } = new();
        
        public readonly EventReactor<ConfigValueChangedEvent> onConfigValueChanged = new();

        public void ModdingEntrypoint() { }

        // Start is called before the first frame update
        private void Awake() {
            Instance = this;
            EventManager.Instance = EventManager;
            DontDestroyOnLoad(gameObject);
            ModdingEntrypoint();
            RunDiscovery();
            foreach (var module in modules) {
                var instance = new ModuleInstance(module);
                ModulesByType[module.GetType()] = instance;
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

        private void RunDiscovery() {
            var assemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .ToList();

            foreach (var eventType in assemblyTypes
                         .Where(type => type.IsSubclassOf(typeof(EventBase)))
                         .Where(type => !type.IsAbstract)) {
                var eventAttribute = eventType.GetCustomAttributes(true).FirstOrDefault(x => x is EventAttribute);
                if (eventAttribute is EventAttribute { DoNotRegister: true }) continue;

                var genericType = typeof(EventReactor<>)
                    .GetGenericTypeDefinition()
                    .MakeGenericType(eventType);

                var reactor = Activator.CreateInstance(genericType) as IEventReactor;
                EventManager.RegisterEvent(reactor);
                Debug.Log($"Registered event {eventType.Name}. EventReactor: {reactor}. Generic: {genericType}");
            }

        }
        
    }
}