using System;
using System.Collections.Generic;
using Spookline.SPC.Events;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class SpookBehaviour : MonoBehaviour, ISpookBehaviour {

        private readonly List<IDisposable> _disposables = new();

        public event Action onStart;
        public event Action onEnable;
        public event Action onDisable;

        protected ISpookBehaviour Ext => this;

        protected virtual void OnDestroy() {
            foreach (var disposable in _disposables) disposable.Dispose();
        }

        protected virtual void Start() {
            onStart?.Invoke();
        }

        protected virtual void OnEnable() {
            onEnable?.Invoke();
        }
        
        protected virtual void OnDisable() {
            onDisable?.Invoke();
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

    public interface IDisposableContainer {

        public void DisposeOnDestroy(IDisposable disposable);
        public void DisposeOnDestroy(Action onDispose) {
            var disposable = new LambdaDisposable(onDispose);
            DisposeOnDestroy(disposable);
        }
        
        public void RemoveOnDestroyDisposal(IDisposable disposable);
    }
    
    public interface ISpookBehaviour : IDisposableContainer, ILifecycleContainer { }
}