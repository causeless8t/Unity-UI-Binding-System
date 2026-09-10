using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleBinder : ComponentBinder<Toggle>, IPropertyBinder<bool>, ICommandBinder<bool>, IEventBinder
    {
        public enum ToggleProperty
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
            public ToggleProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, ToggleProperty> _bindInfoDic; 
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
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        private void OnValueChanged(bool value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case ToggleProperty.Enable: Target.interactable = value; break;
                case ToggleProperty.IsOn: Target.isOn = value; break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case ToggleProperty.Enable: return Target.interactable;
                case ToggleProperty.IsOn: return Target.isOn;
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

        public void InvokeMethod(string key, bool param)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case ToggleProperty.SetWithoutNotify: Target.SetIsOnWithoutNotify(param); break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case ToggleProperty.OnValueChanged: OnValueChangedAction += action as Action<Toggle, bool>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case ToggleProperty.OnValueChanged: OnValueChangedAction -= action as Action<Toggle, bool>; break;
            }
        }
    }
}

