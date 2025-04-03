using System;

namespace Spookline.SPC.Events {
    public abstract class EventBase {

        public void Raise() {
            EventManager.Instance.Raise(this);
        }

    }

    [Event(DoNotRegister = true)]
    public class DummyEvent : EventBase { }

    [Event(DoNotRegister = true)]
    public class VoidEvent : EventBase { }

    public class EventAttribute : Attribute {

        public bool DoNotRegister { get; set; }

    }
}