using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spookline.SPC.Events;
using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class Module<TSelf> : ScriptableObject, IModule, IDisposableContainer where TSelf : Module<TSelf> {

        private static TSelf _instance;
        
        public static bool HasInstance => _instance;
        public static TSelf Instance => _instance;

        private List<IDisposable> _disposables = new();

        public virtual void Load() {
            if (_instance != null) {
                Debug.LogError($"Instance of {typeof(TSelf).Name} already exists.");
            }
        }

        public virtual void Unload() {
            foreach (var disposable in _disposables) disposable.Dispose();
            _instance = null;
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

        public Type GetTypeDelegate() {
            return typeof(TSelf);
        }

    }

    public interface IModule {
  
        public Type GetTypeDelegate();
        
        public void Load();
        public void Unload();

    }

    public class ModuleInstance {

        public IModule module;

        public ModuleInstance(IModule module) {
            this.module = module;
        }

    }
}