using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class SliderBinder : ComponentBinder<Slider>, IPropertyBinder<float>, IPropertyBinder<bool>, IEventBinder
    {
        public enum SliderProperty
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
            public SliderProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, SliderProperty> _bindInfoDic; 
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
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        private void OnValueChanged(float value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, float value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case SliderProperty.Value: Target.value = value; break;
                case SliderProperty.SetWithoutNotify: Target.SetValueWithoutNotify(value); break;
            }
        }

        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case SliderProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case SliderProperty.Enable: return Target.interactable;
            }
            return default;
        }

        public override bool HasKey(string key)
        {
            if (base.HasKey(key)) return true;
            EnsureBindData();
            return _bindInfoDic.ContainsKey(key);
        }
        
        private void EnsureBindData()
        {
            if (_bindInfoDic == null)
                BuildBindings();
        }

        float IPropertyBinder<float>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case SliderProperty.Value: return Target.value;
            }
            return default;
        }

        public void AddListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case SliderProperty.OnValueChanged: OnValueChangedAction += action as Action<Slider, float>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case SliderProperty.OnValueChanged: OnValueChangedAction -= action as Action<Slider, float>; break;
            }
        }
    }
}

