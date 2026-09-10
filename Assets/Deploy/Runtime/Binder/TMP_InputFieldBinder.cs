using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class TMPInputFieldBinder : ComponentBinder<TMP_InputField>,
        IPropertyBinder<string>, IPropertyBinder<bool>, IPropertyBinder<float>, IPropertyBinder<TMP_Text>, IPropertyBinder<int>, ICommandBinder<string>, IEventBinder
    {
        public enum TMP_InputFieldProperty
        {
            Enable,
            Text,
            IsFocused,
            CharacterLimit,
            TextComponent,
            FontSize,
            SetWithoutNotify,
            OnValueChanged,
            OnSubmit,
            OnSelect,
            OnDeselect
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public TMP_InputFieldProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, TMP_InputFieldProperty> _bindInfoDic; 
        private event Action<TMP_InputField, string> OnValueChangedAction;
        private event Action<TMP_InputField, string> OnSubmitAction;
        private event Action<TMP_InputField, string> OnSelectAction;
        private event Action<TMP_InputField, string> OnDeselectAction;

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
                if (info.PropertyType == TMP_InputFieldProperty.OnValueChanged ||
                    info.PropertyType == TMP_InputFieldProperty.OnSubmit ||
                    info.PropertyType == TMP_InputFieldProperty.OnSelect ||
                    info.PropertyType == TMP_InputFieldProperty.OnDeselect)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onSubmit.RemoveListener(OnSubmit);
            Target.onSelect.RemoveListener(OnSelect);
            Target.onDeselect.RemoveListener(OnDeselect);
            Target.onValueChanged.AddListener(OnValueChanged);
            Target.onSubmit.AddListener(OnSubmit);
            Target.onSelect.AddListener(OnSelect);
            Target.onDeselect.AddListener(OnDeselect);
        }

        protected override void OnDestroy()
        {
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onSubmit.RemoveListener(OnSubmit);
            Target.onSelect.RemoveListener(OnSelect);
            Target.onDeselect.RemoveListener(OnDeselect);
            base.OnDestroy();
        }

        public void SetProperty(string key, string value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.Text: Target.text = value; break;
            }
        }

        public void SetProperty(string key, float value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.FontSize: Target.pointSize = value; break;
            }
        }

        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.Enable: Target.interactable = value; break;
            }
        }

        public void SetProperty(string key, TMP_Text value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.TextComponent: Target.textComponent = value; break;
            }
        }

        public void SetProperty(string key, int value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.CharacterLimit: Target.characterLimit = value; break;
            }
        }

        int IPropertyBinder<int>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_InputFieldProperty.CharacterLimit: return Target.characterLimit;
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
        
        TMP_Text IPropertyBinder<TMP_Text>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_InputFieldProperty.TextComponent: return Target.textComponent;
            }
            return default;
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_InputFieldProperty.IsFocused: return Target.isFocused;
                case TMP_InputFieldProperty.Enable: return Target.interactable;
            }
            return default;
        }

        float IPropertyBinder<float>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_InputFieldProperty.FontSize: return Target.pointSize;
            }
            return default;
        }

        string IPropertyBinder<string>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_InputFieldProperty.Text: return Target.text;
            }
            return default;
        }
        
        private void OnValueChanged(string value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }
        
        private void OnSubmit(string value)
        {
            OnSubmitAction?.Invoke(Target, value);
        }
        
        private void OnSelect(string value)
        {
            OnSelectAction?.Invoke(Target, value);
        }
        
        private void OnDeselect(string value)
        {
            OnDeselectAction?.Invoke(Target, value);
        }

        public void InvokeMethod(string key, string param)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_InputFieldProperty.SetWithoutNotify: Target.SetTextWithoutNotify(param); break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case TMP_InputFieldProperty.OnValueChanged: OnValueChangedAction += action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnSubmit: OnSubmitAction += action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnSelect: OnSelectAction += action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnDeselect: OnDeselectAction += action as Action<TMP_InputField, string>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case TMP_InputFieldProperty.OnValueChanged: OnValueChangedAction -= action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnSubmit: OnSubmitAction -= action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnSelect: OnSelectAction -= action as Action<TMP_InputField, string>; break;
                case TMP_InputFieldProperty.OnDeselect: OnDeselectAction -= action as Action<TMP_InputField, string>; break;
            }
        }
    }
}

