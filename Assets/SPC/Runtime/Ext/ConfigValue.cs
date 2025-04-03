using System;
using Spookline.SPC.Events;
using UnityEngine;

namespace Spookline.SPC.Ext {
    public class ConfigValue<T> : ISpookInstanceMember {

        public Type moduleType;
        public string fieldName;

        public ConfigValue(Type moduleType, string fieldName) {
            this.moduleType = moduleType;
            this.fieldName = fieldName;
            Debug.Log($"ConfigValue created for {moduleType.Name}.{fieldName}");
        }

        public static ConfigValue<T> Ref<TModule>(string fieldName) {
            return new ConfigValue<T>(typeof(TModule), fieldName);
        }

        private T _overrideValue;
        private T _originalValue;
        private bool _hasValue;
        private bool _hasOverride;

        public void ApplyOverride(T value) {
            _overrideValue = value;
            _hasOverride = true;
        }

        public void ResetOverride() {
            _overrideValue = default;
            _hasOverride = false;
        }

        public T GetValue() {
            if (_hasOverride) {
                return _overrideValue;
            }

            if (!_hasValue) {
                _originalValue = (T)Globals.Instance.ModulesByType[moduleType].configValueGetters[fieldName].Invoke();
                _hasValue = true;
            }

            return _originalValue;
        }

        public override string ToString() {
            return GetValue().ToString();
        }

        public static implicit operator T(ConfigValue<T> configValue) {
            return configValue.GetValue();
        }

        public void InstanceAwake(SpookInstance instance) {
            try {
                var value = Globals.Instance.ModulesByType[moduleType].configValueGetters[fieldName].Invoke();
                _originalValue = (T)value;
                Globals.Instance.onConfigValueChanged.Subscribe(_OnConfigValueChanged);
                Debug.Log($"Config value {moduleType.Name}.{fieldName} = {value}");
            }
            catch (Exception e) {
                Debug.LogError($"Failed to get config value for {moduleType.Name}.{fieldName}: {e}");
            }
        }

        private void _OnConfigValueChanged(ConfigValueChangedEvent args) {
            Debug.Log($"Config value changed: {args.ModuleType.Name}.{args.FieldName} = {args.Value}");
            if (args.ModuleType == moduleType && args.FieldName == fieldName) {
                _originalValue = (T)args.Value;
            }
        }

        public void InstanceDispose(SpookInstance instance) {
            Globals.Instance.onConfigValueChanged.Unsubscribe(_OnConfigValueChanged);
            _originalValue = default;
            _overrideValue = default;
        }

    }

    public class ConfigValueChangedEvent : EventBase {

        public Type ModuleType { get; set; }
        public string FieldName { get; set; }
        public object Value { get; set; }

    }

    public class ConfigValueAttribute : Attribute { }
}