using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Spookline.SPC.Events {
    public abstract class Evt<TSelf> : EvtBase where TSelf : Evt<TSelf> {

        public TSelf Raise() {
            EventReactor<TSelf>.Shared.Raise((TSelf)this);
            return (TSelf)this;
        }

        public static HandlerRegistration<TSelf> Subscribe(EventHandler<TSelf> action,
            int priority = 0, string debugName = null) {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<TSelf>.Shared.Subscribe(action, priority, debugName);
        }

        public static HandlerRegistration<TSelf> SubscribeOnce(EventHandler<TSelf> action,
            int priority = 0, string debugName = null) {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<TSelf>.Shared.SubscribeOnce(action, priority, debugName);
        }

        public static HandlerRegistration<TSelf> SubscribeStream(StreamEventHandler<TSelf> action,
            int priority = 0, string debugName = null) {
#if DEBUG
            debugName ??= EventReactorInfo.GetDebugName(action);
#endif
            return EventReactor<TSelf>.Shared.SubscribeStream(action, priority, debugName);
        }

    }

    public abstract class AsyncChainEvt<TSelf> : Evt<TSelf> where TSelf : AsyncChainEvt<TSelf> {

        private readonly LinkedList<Func<UniTask>> _chain = new();

        public event Func<UniTask> Chain {
            add => _chain.AddLast(value);
            remove => throw new NotSupportedException();
        }

        public async UniTask<TSelf> RaiseAsync() {
            Raise();
            foreach (var action in _chain) {
                if (action == null) continue;
                try {
                    await action.Invoke();
                } catch (Exception e) {
                    Debug.LogError($"Error in async chain for event {typeof(TSelf).Name}: {e}");
                }
            }

            return (TSelf)this;
        }

    }

    public abstract class EvtBase {

        private readonly LinkedList<Action> _finalizerChain = new();

        public void AddFinalizer(Action finalizer) {
            _finalizerChain.AddLast(finalizer);
        }

        internal void InvokeFinalizers() {
            foreach (var action in _finalizerChain) action?.Invoke();
        }

    }

    [Event(DoNotRegister = true)]
    public class DummyEvt : Evt<DummyEvt> { }

    [Event(DoNotRegister = true)]
    public class VoidEvt : Evt<VoidEvt> { }

    public class EventAttribute : Attribute {

        public bool DoNotRegister { get; set; }

    }
}