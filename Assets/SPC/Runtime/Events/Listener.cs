using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spookline.SPC.Events {
    /// <summary>
    ///     Extendable class for easily wrapping methods to event reactor delegates.
    ///     Can be registered via <see cref="EventManager.RegisterListener" />
    /// </summary>
    public abstract class Listener {

        private EventManager _managerReference;
        private readonly Dictionary<Type, List<object>> _subscriptions = new();

        internal void RegisterAll(EventManager manager) {
            _managerReference = manager;
            foreach (var methodInfo in GetType().GetMethods()
                         .Where(method => method.GetCustomAttribute<EventHandlerAttribute>() is not null)) {
                var attribute = methodInfo.GetCustomAttribute<EventHandlerAttribute>();
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1) throw new Exception("EventHandler must have a single Event parameter");
                var eventParameter = parameters[0];
                var eventType = eventParameter.ParameterType;
                var reactor = manager.GetUnsafe(eventType);
                var subscribeUnsafe = reactor.SubscribeUnsafe(this, methodInfo, attribute.Priority);

                // Add the subscription to the list of subscriptions
                if (!_subscriptions.TryGetValue(eventType, out var list)) {
                    list = new List<object>();
                }

                list.Add(subscribeUnsafe);
                _subscriptions[eventType] = list;

                Debug.Log($"Subscribed {methodInfo.Name} to {eventType.Name}");
            }
        }

        /// <summary>
        ///     Unregisters all handlers of the listener.
        /// </summary>
        /// <exception cref="Exception">if the listener is not linked</exception>
        public void UnregisterAll() {
            if (_managerReference != null) {
                foreach (var subscription in _subscriptions) {
                    var reactor = _managerReference.GetUnsafe(subscription.Key);
                    foreach (var handlers in subscription.Value) {
                        reactor.UnsubscribeUnsafe(handlers);
                    }
                }

                _subscriptions.Clear();
            }
            else {
                throw new Exception("No manager is reference");
            }
        }

    }

    /// <summary>
    /// Mixin for easily registering and unregistering event handlers.
    /// </summary>
    public interface IListener {

        public Dictionary<Type, List<object>> Subscriptions { get; }

        public void RegisterAll(object instance, Type type) {
            var manager = EventManager.Instance;
            foreach (var methodInfo in type.GetMethods()
                         .Where(method => method.GetCustomAttribute<EventHandlerAttribute>() is not null)) {
                var attribute = methodInfo.GetCustomAttribute<EventHandlerAttribute>();
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1) throw new Exception("EventHandler must have a single Event parameter");
                var eventParameter = parameters[0];
                var eventType = eventParameter.ParameterType;
                var reactor = manager.GetUnsafe(eventType);
                var subscribeUnsafe = reactor.SubscribeUnsafe(instance, methodInfo, attribute.Priority);

                // Add the subscription to the list of subscriptions
                if (!Subscriptions.TryGetValue(eventType, out var list)) {
                    list = new List<object>();
                }

                list.Add(subscribeUnsafe);
                Subscriptions[eventType] = list;

                Debug.Log($"Subscribed {methodInfo.Name} to {eventType.Name}");
            }
        }

        public void UnregisterAll() {
            foreach (var subscription in Subscriptions) {
                var reactor = EventManager.Instance.GetUnsafe(subscription.Key);
                foreach (var handler in subscription.Value) {
                    var debugName = reactor.GetDebugName(handler);
                    reactor.UnsubscribeUnsafe(handler);
                    Debug.Log($"Unsubscribed {debugName} from {subscription.Key.Name}");
                }
            }

            Subscriptions.Clear();
        }

    }

    public class MonoListener : MonoBehaviour, IListener {

        public Dictionary<Type, List<object>> Subscriptions { get; } = new();

        public virtual void Awake() {
            (this as IListener).RegisterAll(this, GetType());
        }

        public virtual void OnDestroy() {
            (this as IListener).UnregisterAll();
        }

    }

    public abstract class ManagerBehaviour<TSelf> : MonoBehaviour, IListener where TSelf : ManagerBehaviour<TSelf> {

        public static bool HasInstance => Instance;

        public static TSelf Instance { get; private set; }

        public Dictionary<Type, List<object>> Subscriptions { get; } = new();

        public virtual void Awake() {
            Instance = (TSelf)this;
        }

        public virtual void OnEnable() {
            (this as IListener).RegisterAll(this, GetType());
        }

        public virtual void OnDisable() {
            (this as IListener).UnregisterAll();
        }

        public virtual void OnDestroy() {
            Instance = null;
            (this as IListener).UnregisterAll();
        }

    }

    public class EventHandlerAttribute : Attribute {

        public int Priority { get; set; } = 0;

    }
}