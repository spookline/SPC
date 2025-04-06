using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spookline.SPC.Events;
using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class Module : ScriptableObject, IDisposableContainer {
        
        private List<IDisposable> _disposables = new();
        
        public virtual void Load() { }
        
        public virtual void Unload() {
            foreach (var disposable in _disposables) disposable.Dispose();
        }

        public void DisposeOnDestroy(IDisposable disposable) {
            _disposables.Add(disposable);
        }

        public void RemoveOnDestroyDisposal(IDisposable disposable) {
            _disposables.Remove(disposable);
        }
        
        public EventCallbackBuilder<T> On<T>() where T : Evt<T> {
            return new EventCallbackBuilder<T>(this);
        }
    }

    
    [Serializable]
    public class ModuleConfigData {

        public string[] names;
        public string[] values;

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
        }

    }
}