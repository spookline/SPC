using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spookline.SPC.Events;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public abstract class SpookBehaviour : MonoBehaviour {

        private SpookInstance _spookInstance;

        protected virtual void Awake() {
            _spookInstance = SpookInstance.Create(this);
            _spookInstance.Awake();
        }

        protected virtual void OnDestroy() {
            _spookInstance.Dispose();
        }

    }

    public interface ISpookInstanceMember {

        public void InstanceAwake(SpookInstance instance) { }
        public void InstanceDispose(SpookInstance instance) { }

    }

    public class SpookInstance : IDisposable, IListener {

        public Component component;
        private List<ISpookInstanceMember> _members = new();
        public Dictionary<Type, List<object>> Subscriptions { get; } = new();

        public void Awake() {
            foreach (var member in _members) {
                member.InstanceAwake(this);
            }

            (this as IListener).RegisterAll(component, component.GetType());
        }

        public void Dispose() {
            foreach (var member in _members) {
                member.InstanceDispose(this);
            }

            (this as IListener).UnregisterAll();
        }

        public static SpookInstance Create(Component instance) {
            var spookInstance = new SpookInstance();
            spookInstance.component = instance;
            var type = instance.GetType();
            var fields = type.GetRuntimeFields().ToList();

            foreach (var field in fields) {
                var fieldType = field.FieldType;
                if (fieldType.GetInterfaces().Contains(typeof(ISpookInstanceMember))) {
                    var configValue = field.GetValue(instance) as ISpookInstanceMember;
                    spookInstance._members.Add(configValue);
                    Debug.Log($"Found config value {field.Name} in {instance.GetType().Name}");
                }
            }

            return spookInstance;
        }

    }
}