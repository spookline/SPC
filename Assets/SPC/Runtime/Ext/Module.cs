using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spookline.SPC.Events;
using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class Module : ScriptableObject {

        public virtual void Load() { }

        public virtual void Unload() { }


        [ContextMenu("Update Config Values")]
        private void UpdateConfigValues() {
            var core = Globals.Instance;
            var moduleType = GetType();
            foreach (var (key, getter) in core.ModulesByType[moduleType].configValueGetters) {
                core.onConfigValueChanged.Raise(new ConfigValueChangedEvent() {
                    ModuleType = moduleType,
                    FieldName = key,
                    Value = getter.Invoke()
                });
            }
        }

    }

    public class ModuleInstance {

        public Module module;
        public Dictionary<string, GetConfigValueDelegate> configValueGetters = new();

        public ModuleInstance(Module module) {
            this.module = module;
            Initialize();
        }

        public delegate object GetConfigValueDelegate();

        private void Initialize() {
            var type = module.GetType();
            var fields = type.GetRuntimeFields().ToList();

            foreach (var field in fields) {
                if (field.GetCustomAttribute<ConfigValueAttribute>() != null) {
                    configValueGetters[field.Name] = () => field.GetValue(module);
                }
            }
        }

    }
}