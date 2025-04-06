using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spookline.SPC.Events {
    /// <summary>
    ///     .Net event based proxy event handler and invocator.
    ///     All events used by the class should have mutable public properties for their data.
    ///     Event subscribers should manipulate the properties of the event they receive to alter data.
    /// </summary>
    /// <typeparam name="T">type of the event</typeparam>
    public class EventReactor<T> : IEventReactor where T : Evt<T> {

        private static EventReactor<T> _globalReactor;

        public static EventReactor<T> Shared => _globalReactor ??= EventManager.Instance.RegisterEvent<T>();


        private readonly HandlerRegistrationComparer<T> _comparer = new();
        private List<HandlerRegistration<T>> _registrations = new();

        /// <summary>
        ///     Returns the type of the generic <see cref="T" />.
        /// </summary>
        public Type TypeDelegate() {
            return typeof(T);
        }

        /// <summary>
        ///     Invokes the multicast event system.
        ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
        /// </summary>
        /// <param name="obj">the delegate boxed as an object</param>
        public void RaiseUnsafe(object obj) {
            Raise((T)obj);
        }

        /// <summary>
        ///     Subscribes a method to the backing event.
        ///     Uses reflections to create method delegates of type <see cref="T" />
        ///     which can be subscribed normally.
        /// </summary>
        /// <param name="obj">the instance of object which method shall be hooked</param>
        /// <param name="info">the method which shall be hooked</param>
        /// <param name="priority">the priority of the subscription</param>
        public object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0) {
            var handler = DelegateUtils.CreateDelegate<EventHandler<T>>(obj, info);

#if DEBUG
            Subscribe(handler, priority, $"{info.DeclaringType?.Name ?? "@"}.{info.Name}");
#else
            Subscribe(handler, priority);
#endif

            return handler;
        }

        /// <summary>
        ///     Unsubscribes a delegate from the backing event
        /// </summary>
        /// <param name="subscription">the delegate boxed as an object</param>
        public void UnsubscribeUnsafe(object subscription) {
            Unsubscribe(subscription as EventHandler<T>);
        }

        public string ResolveDebugName(object subscription) {
            lock (this) {
                return _registrations.FirstOrDefault(x => x.Handler == subscription as EventHandler<T>)?.DebugName;
            }
        }

        public EventReactorInfo CreateInfo() {
            var priorityRows = new List<EventReactorInfo.PriorityRow>();
            lock (this) {
                foreach (var group in _registrations.GroupBy(x => x.Priority)) {
                    var handlers = group
                        .GroupBy(y => y.DebugName)
                        .Select(g => g.Count() == 1 ? g.Key : $"{g.Key} ({g.Count()})")
                        .ToList();

                    priorityRows.Add(new EventReactorInfo.PriorityRow {
                        Priority = group.Key,
                        Handlers = handlers
                    });
                }
            }

            return new EventReactorInfo {
                Name = typeof(T).Name,
                Type = typeof(T),
                Rows = priorityRows
            };
        }

        /// <summary>
        ///     Invokes the multicast event system.
        ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
        /// </summary>
        /// <param name="evt">the event argument object</param>
        public void Raise(T evt) {
            lock (this) {
                foreach (var registration in _registrations) registration.Handler.Invoke(evt);
                evt.InvokeFinalizers();
            }
        }

        /// <summary>
        ///     Subscribes a delegate to the backing event.
        /// </summary>
        /// <param name="handler">the delegate to subscribe</param>
        /// <param name="priority">the priority of the subscription</param>
        /// <param name="debugName">The debug name of the subscription</param>
        public HandlerRegistration<T> Subscribe(EventHandler<T> handler, int priority = 0, string debugName = null) {
            lock (this) {
                debugName ??= ResolveDebugName(handler);
                var registration = new HandlerRegistration<T>(this, priority, handler, debugName);
                _registrations = _registrations
                    .Append(registration)
                    .OrderBy(x => x.Priority)
                    .ToList();
                return registration;
            }
        }

        /// <summary>
        ///     Subscribes a delegate to the backing event.
        /// </summary>
        /// <param name="handler">the delegate to subscribe</param>
        /// <param name="priority">the priority of the subscription</param>
        /// <param name="debugName">The debug name of the subscription</param>
        public HandlerRegistration<T>
            SubscribeOnce(EventHandler<T> handler, int priority = 0, string debugName = null) {
            lock (this) {
                var consumer = new SingleConsumer(handler);
                var registration = Subscribe(consumer.Handle, priority, debugName);
                consumer.registration = registration;
                return registration;
            }
        }

        /// <summary>
        ///     Subscribes a stream handler delegate to the backing event.
        ///     The handler will be invoked and if it returns true, it will be unsubscribed automatically.
        /// </summary>
        /// <param name="handler">the stream handler delegate to subscribe</param>
        /// <param name="priority">the priority of the subscription</param>
        /// <param name="debugName">The debug name of the subscription</param>
        public HandlerRegistration<T> SubscribeStream(StreamEventHandler<T> handler, int priority = 0,
            string debugName = null) {
            lock (this) {
                var consumer = new StreamConsumer(handler);
                var registration = Subscribe(consumer.Handle, priority, debugName);
                consumer.registration = registration;
                return registration;
            }
        }

        /// <summary>
        ///     Unsubscribes a delegate from the backing event.
        /// </summary>
        /// <param name="handler">the delegate to unsubscribe</param>
        public void Unsubscribe(EventHandler<T> handler) {
            lock (this) {
                _registrations.RemoveAll(x => x.Handler == handler);
            }
        }

        /// <summary>
        ///     Unsubscribes a delegate from the backing event.
        /// </summary>
        /// <param name="registration">the registration to unsubscribe</param>
        public void Unsubscribe(HandlerRegistration<T> registration) {
            lock (this) {
                _registrations.Remove(registration);
            }
        }

        private class SingleConsumer {

            private readonly EventHandler<T> _handler;
            internal HandlerRegistration<T> registration;

            public SingleConsumer(EventHandler<T> handler) {
                _handler = handler;
            }

            public void Handle(T evt) {
                _handler.Invoke(evt);
                evt.AddFinalizer(registration.Dispose);
            }

        }

        private class StreamConsumer {

            private readonly StreamEventHandler<T> _handler;
            internal HandlerRegistration<T> registration;

            public StreamConsumer(StreamEventHandler<T> handler) {
                _handler = handler;
            }

            public void Handle(T evt) {
                var result = _handler.Invoke(evt);
                if (result) {
                    evt.AddFinalizer(registration.Dispose);
                }
            }

        }

    }

    internal class DelegateUtils {

        public static T CreateDelegate<T>(MethodInfo info) where T : Delegate {
            var delegateType = typeof(T);
            var delegated = info.CreateDelegate(delegateType);
            return (T)delegated;
        }

        public static T CreateDelegate<T>(object instance, MethodInfo info) where T : Delegate {
            var delegateType = typeof(T);
            var delegated = info.CreateDelegate(delegateType, instance);
            return (T)delegated;
        }

    }

    public class HandlerRegistration<T> : IDisposable where T : Evt<T> {

        public int Priority { get; }
        public EventHandler<T> Handler { get; }
        public string DebugName { get; }
        public EventReactor<T> Reactor { get; private set; }

        public HandlerRegistration(EventReactor<T> reactor, int priority, EventHandler<T> handler,
            string debugName) {
            Priority = priority;
            Handler = handler;
            DebugName = debugName ?? "unknown";
            Reactor = reactor;
        }

        public void Dispose() {
            Reactor?.Unsubscribe(this);
            Reactor = null;
        }

    }

    public class HandlerRegistrationComparer<T> : IComparer<HandlerRegistration<T>> where T : Evt<T> {

        public int Compare(HandlerRegistration<T> x, HandlerRegistration<T> y) {
            return x.Priority.CompareTo(y.Priority);
        }

    }

    public static class VoidEventExtension {

        public static readonly VoidEvt RecyclableEvt = new();

        /// <summary>
        ///     Invokes the multicast event system.
        ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
        /// </summary>
        public static void Raise(this EventReactor<VoidEvt> reactor) {
            reactor.Raise(RecyclableEvt);
        }

        /// <summary>
        ///     Subscribes a method to the backing event.
        ///     Uses reflections to create method delegates of type T
        ///     which can be subscribed normally. Uses an inlined delegate
        ///     to wrap the action into the matching VoidEvent delegate type.
        /// </summary>
        /// <param name="reactor">the reactor to subscribe to</param>
        /// <param name="obj">the instance of object which method shall be hooked</param>
        /// <param name="info">the method which shall be hooked</param>
        public static object SubscribeAction(this EventReactor<VoidEvt> reactor, object obj, MethodInfo info) {
            var action = DelegateUtils.CreateDelegate<Action>(obj, info);
            EventHandler<VoidEvt> handler = _ => action.Invoke();
            reactor.Subscribe(handler);
            return handler;
        }

    }

    public delegate void EventHandler<in T>(T args) where T : Evt<T>;

    public delegate bool StreamEventHandler<in T>(T args) where T : Evt<T>;

    public interface IEventReactor {

        Type TypeDelegate();
        void RaiseUnsafe(object obj);
        object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0);
        void UnsubscribeUnsafe(object subscription);
        string ResolveDebugName(object subscription);
        EventReactorInfo CreateInfo();

    }

    public class EventReactorInfo {

        public string Name { get; set; }
        public Type Type { get; set; }
        public List<PriorityRow> Rows { get; set; } = new();

        public class PriorityRow {

            public int Priority { get; set; }
            public List<string> Handlers { get; set; } = new();

        }
        public static string GetDebugName(Delegate handler) {
            try {
                var method = handler.Method;
                var declaringType = method.DeclaringType;
                var name = method.Name;
                return $"{declaringType?.Name ?? "@"}.{name}";
            } catch (MissingMemberException e) {
                return "Private Method";
            }
        }

    }
}
