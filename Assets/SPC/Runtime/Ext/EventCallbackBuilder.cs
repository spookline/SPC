using Spookline.SPC.Events;

namespace Spookline.SPC.Ext
{
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