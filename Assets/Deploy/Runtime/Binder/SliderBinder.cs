using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class SliderBinder : ComponentBinder<Slider>, IPropertyBinder<float>, IPropertyBinder<bool>, IEventBinder
    {
        public enum BindingType
        {
            Value,
            SetWithoutNotify,
            Enable,
            OnValueChanged
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
        private event Action<Slider, float> OnValueChangedAction;

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

        private void OnValueChanged(float value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, float value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Value: target.value = value; break;
                case BindingType.SetWithoutNotify: target.SetValueWithoutNotify(value); break;
            }
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
            }
            return default;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        float IPropertyBinder<float>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.Value: return target.value;
            }
            return default;
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<Slider, float> callback)
                OnValueChangedAction += callback;
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.OnValueChanged)
                return;

            if (action is Action<Slider, float> callback)
                OnValueChangedAction -= callback;
        }
    }
}

