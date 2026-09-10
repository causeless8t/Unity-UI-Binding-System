using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class DropdownBinder : ComponentBinder<TMP_Dropdown>, IPropertyBinder<bool>, IPropertyBinder<int>, IPropertyBinder<List<TMP_Dropdown.OptionData>>, ICommandBinder<int>, IEventBinder
    {
        public enum DropdownProperty
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
            public DropdownProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, DropdownProperty> _bindInfoDic; 
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

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }
        
        public override string[] GetKeyList()
        {
            List<string> result = new();
            _bindInfos.ForEach((info) =>
            {
                if (info.PropertyType == DropdownProperty.OnValueChanged)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        private void OnValueChanged(int value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case DropdownProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case DropdownProperty.Enable: return Target.interactable;
            }
            return default;
        }
        
        public void SetProperty(string key, int value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case DropdownProperty.Value: Target.value = value; break;
            }
        }

        int IPropertyBinder<int>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case DropdownProperty.Value: return Target.value;
            }
            return default;
        }
        
        public void SetProperty(string key, List<TMP_Dropdown.OptionData> value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case DropdownProperty.OptionList: Target.options = value; break;
            }
        }

        List<TMP_Dropdown.OptionData> IPropertyBinder<List<TMP_Dropdown.OptionData>>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            switch (type)
            {
                case DropdownProperty.OptionList: return Target.options;
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
                LoadData();
        }

        public void InvokeMethod(string key, int param)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case DropdownProperty.SetWithoutNotify: Target.SetValueWithoutNotify(param); break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case DropdownProperty.OnValueChanged: OnValueChangedAction += action as Action<TMP_Dropdown, int>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case DropdownProperty.OnValueChanged: OnValueChangedAction -= action as Action<TMP_Dropdown, int>; break;
            }
        }
    }
}

