using System;
using System.Collections.Generic;

namespace Spookline.SPC.Events {
    public class EventManager {

        public static EventManager Instance = new();

        public Dictionary<Type, IEventReactor> Reactors = new();

        /// <summary>
        ///     Registers an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor which shall be subscribed</typeparam>
        public EventReactor<T> RegisterEvent<T>() where T : Evt<T> {
            if (Reactors.TryGetValue(typeof(T), out var eventReactor)) {
                return (EventReactor<T>)eventReactor;
            }

            var reactor = new EventReactor<T>();
            Reactors[typeof(T)] = reactor;
            return reactor;
        }

        /// <summary>
        ///     Registers an event reactor.
        /// </summary>
        public void RegisterEvent(IEventReactor reactor) {
            if (Reactors.ContainsKey(reactor.TypeDelegate())) {
                return;
            }

            Reactors[reactor.TypeDelegate()] = reactor;
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        /// <param name="eventType">type of the reactor which shall be unsubscribed</param>
        public void UnregisterEvent(Type eventType) {
            Reactors.Remove(eventType);
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        public void UnregisterEvent(IEventReactor reactor) {
            Reactors.Remove(reactor.TypeDelegate());
        }

        /// <summary>
        ///     Unregisters an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor which shall be unsubscribed</typeparam>
        public void UnregisterEvent<T>() {
            Reactors.Remove(typeof(T));
        }

        /// <summary>
        ///     Retrieves an event reactor.
        /// </summary>
        /// <typeparam name="T">type of the reactor</typeparam>
        public EventReactor<T> Get<T>() where T : Evt<T> {
            return (EventReactor<T>)Reactors[typeof(T)];
        }

        /// <summary>
        ///     Retrieves an event reactor.
        /// </summary>
        /// <param name="type">type of the reactor</param>
        public IEventReactor GetUnsafe(Type type) {
            return Reactors[type];
        }

        /// <summary>
        ///     Retrieves the reactor related to the event and invokes the multicast event system.
        ///     See <see cref="EventReactor{T}" /> for details about required property attributes.
        /// </summary>
        /// <param name="evt">the event argument object</param>
        public void Raise<T>(T evt) where T : Evt<T> {
            Reactors[evt.GetType()].RaiseUnsafe(evt);
        }

    }
}