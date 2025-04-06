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

    public class ModuleInstance {

        public Module module;

        public ModuleInstance(Module module) {
            this.module = module;
        }
    }
}