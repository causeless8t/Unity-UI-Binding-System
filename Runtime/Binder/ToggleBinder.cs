using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleBinder : ComponentBinder<Toggle>, IPropertyBinder<bool>, ICommandBinder<bool>, IEventBinder
    {
        public enum BindingType
        {
            IsOn,
            Enable,
            OnValueChanged,
            SetWithoutNotify
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public BindingType bindingTypeType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private readonly BindingMap<BindingType> _bindingMap = new(); 
        private event Action<Toggle, bool> OnValueChangedAction;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroy()
        {
            Target.onValueChanged.RemoveListener(OnValueChanged);
            base.OnDestroy();
        }

        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.bindingTypeType, this);
            }
        }

        private void OnValueChanged(bool value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Enable: target.interactable = value; break;
                case BindingType.IsOn: target.isOn = value; break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.Enable: return target.interactable;
                case BindingType.IsOn: return target.isOn;
            }
            return default;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        public void InvokeMethod(string key, bool param)
        {
            if (!_bindingMap.TryGet(key, out var type))
                return;

            var target = GetTarget();
            if (target == null)
                return;

            switch (type)
            {
                case BindingType.SetWithoutNotify:
                    target.SetIsOnWithoutNotify(param);
                    break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<Toggle, bool> callback)
                OnValueChangedAction += callback;
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<Toggle, bool> callback)
                OnValueChangedAction -= callback;
        }
    }
}

