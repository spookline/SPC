using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spookline.SPC.Events;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class SpookBehaviour : MonoBehaviour, IDisposableContainer {

        private readonly List<IDisposable> _disposables = new();

        protected virtual void OnDestroy() {
            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }
        }

        public EventCallbackBuilder<T> On<T>() where T : Evt<T> {
            return new EventCallbackBuilder<T>(this);
        }

        public void DisposeOnDestroy(IDisposable disposable) {
            _disposables.Add(disposable);
        }

        public void RemoveOnDestroyDisposal(IDisposable disposable) {
            _disposables.Remove(disposable);
        }

    }

    public interface IDisposableContainer {

        public void DisposeOnDestroy(IDisposable disposable);
        public void RemoveOnDestroyDisposal(IDisposable disposable);

    }

    public readonly struct EventCallbackBuilder<T> where T : Evt<T> {

        private readonly IDisposableContainer _container;

        public EventCallbackBuilder(IDisposableContainer container) {
            _container = container;
        }

        public HandlerRegistration<T> Do(Events.EventHandler<T> action, int priority = 0, string debugName = null) {
#if DEBUG
            if (debugName == null) {
                var clazz = _container.GetType().FullName;
                var name = EventReactorInfo.GetDebugName(action);
                debugName = $"@{clazz} {name}";
            }
#endif

            var registration = EventReactor<T>.Shared.Subscribe(action, priority, debugName);
            _container.DisposeOnDestroy(registration);
            return registration;
        }

        public HandlerRegistration<T> Stream(Events.StreamEventHandler<T> action, int priority = 0,
            string debugName = null) {
#if DEBUG
            if (debugName == null) {
                var clazz = _container.GetType().FullName;
                var name = EventReactorInfo.GetDebugName(action);
                debugName = $"@{clazz} {name}";
            }
#endif
            var registration = EventReactor<T>.Shared.SubscribeStream(action, priority, debugName);
            _container.DisposeOnDestroy(registration);
            return registration;
        }

        public HandlerRegistration<T> DoOnce(Events.EventHandler<T> action, int priority = 0, string debugName = null) {
#if DEBUG
            if (debugName == null) {
                var clazz = _container.GetType().FullName;
                var name = EventReactorInfo.GetDebugName(action);
                debugName = $"@{clazz} {name}";
            }
#endif
            var registration = EventReactor<T>.Shared.SubscribeOnce(action, priority, debugName);
            _container.DisposeOnDestroy(registration);
            return registration;
        }

    }
}