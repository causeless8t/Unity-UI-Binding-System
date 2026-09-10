using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class DropdownBinder : ComponentBinder<TMP_Dropdown>, IPropertyBinder<bool>, IPropertyBinder<int>, IPropertyBinder<List<TMP_Dropdown.OptionData>>, ICommandBinder<int>, IEventBinder
    {
        public enum BindingType
        {
            Value,
            Enable,
            OptionList,
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
        private BindingMap<BindingType> _bindingMap; 
        private event Action<TMP_Dropdown, int> OnValueChangedAction;

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

        private void OnValueChanged(int value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.Enable)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.interactable = value;
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return false;
            
            var target = GetTarget();
            if (target == null)
                return false;

            return property == BindingType.Enable && target.interactable;
        }
        
        public void SetProperty(string key, int value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.Value)
                return;
            
            var target = GetTarget();
            if (target == null)
                return;

            target.value = value;
        }

        int IPropertyBinder<int>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.Value ? target.value : default;
        }
        
        public void SetProperty(string key, List<TMP_Dropdown.OptionData> value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OptionList)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.options = value;
        }

        List<TMP_Dropdown.OptionData> IPropertyBinder<List<TMP_Dropdown.OptionData>>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.OptionList ? target.options : default;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        public void InvokeMethod(string key, int param)
        {
            if (!_bindingMap.TryGet(key, out var type))
                return;

            var target = GetTarget();
            if (target == null)
                return;

            switch (type)
            {
                case BindingType.SetWithoutNotify:
                    target.SetValueWithoutNotify(param);
                    break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<TMP_Dropdown, int> callback)
                OnValueChangedAction += callback;
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<TMP_Dropdown, int> callback)
                OnValueChangedAction -= callback;
        }
    }
}

