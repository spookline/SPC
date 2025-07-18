using System;
using System.Collections.Generic;
using Spookline.SPC.Events;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class SpookBehaviour : MonoBehaviour, IDisposableContainer {

        private readonly List<IDisposable> _disposables = new();

        protected virtual void OnDestroy() {
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

    public interface IDisposableContainer {

        public void DisposeOnDestroy(IDisposable disposable);
        public void RemoveOnDestroyDisposal(IDisposable disposable);

    }
}