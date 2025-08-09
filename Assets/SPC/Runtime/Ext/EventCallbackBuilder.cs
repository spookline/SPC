using System;
using Cysharp.Threading.Tasks;
using Spookline.SPC.Events;

namespace Spookline.SPC.Ext {
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

        public HandlerRegistration<T> Stream(StreamEventHandler<T> action, int priority = 0,
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


    public static class EventCallbackBuilderExtensions {

        public static HandlerRegistration<T> Do<T>(this EventCallbackBuilder<T> builder, Func<T, UniTask> action,
            int priority = 0, string debugName = null) where T : Evt<T> =>
            builder.Do(evt => { action(evt).Forget(); }, priority, debugName);

        public static HandlerRegistration<T> AsyncDo<T>(this EventCallbackBuilder<T> builder, Func<T, UniTask> action,
            int priority = 0, string debugName = null) where T : AsyncChainEvt<T> {
            return builder.Do(evt => { evt.Chain += async () => { await action(evt); }; }, priority, debugName);
        }

    }
}