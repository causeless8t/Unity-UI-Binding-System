using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class TMPInputFieldBinder : ComponentBinder<TMP_InputField>,
        IPropertyBinder<string>, IPropertyBinder<bool>, IPropertyBinder<float>, IPropertyBinder<TMP_Text>, IPropertyBinder<int>, ICommandBinder<string>, IEventBinder
    {
        public enum BindingType
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
            public BindingType bindingTypeType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private readonly BindingMap<BindingType> _bindingMap = new(); 
        private event Action<TMP_InputField, string> OnValueChangedAction;
        private event Action<TMP_InputField, string> OnSubmitAction;
        private event Action<TMP_InputField, string> OnSelectAction;
        private event Action<TMP_InputField, string> OnDeselectAction;

        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.bindingTypeType, this);
            }
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
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.Text)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.text = value;
        }

        public void SetProperty(string key, float value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.FontSize)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.pointSize = value;
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

        public void SetProperty(string key, TMP_Text value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.TextComponent)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.textComponent = value;
        }

        public void SetProperty(string key, int value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.CharacterLimit)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.characterLimit = value;
        }

        int IPropertyBinder<int>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.CharacterLimit ? target.characterLimit : default;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }
        
        TMP_Text IPropertyBinder<TMP_Text>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.TextComponent ? target.textComponent : default;
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
                case BindingType.IsFocused: return target.isFocused;
                case BindingType.Enable: return target.interactable;
            }
            return default;
        }

        float IPropertyBinder<float>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.FontSize ? target.pointSize : default;
        }

        string IPropertyBinder<string>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.Text ? target.text : default;
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
            if (!_bindingMap.TryGet(key, out var type))
                return;

            var target = GetTarget();
            if (target == null)
                return;

            switch (type)
            {
                case BindingType.SetWithoutNotify:
                    target.SetTextWithoutNotify(param);
                    break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            switch (property)
            {
                case BindingType.OnValueChanged: OnValueChangedAction += action as Action<TMP_InputField, string>; break;
                case BindingType.OnSubmit: OnSubmitAction += action as Action<TMP_InputField, string>; break;
                case BindingType.OnSelect: OnSelectAction += action as Action<TMP_InputField, string>; break;
                case BindingType.OnDeselect: OnDeselectAction += action as Action<TMP_InputField, string>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;
            
            switch (property)
            {
                case BindingType.OnValueChanged: OnValueChangedAction -= action as Action<TMP_InputField, string>; break;
                case BindingType.OnSubmit: OnSubmitAction -= action as Action<TMP_InputField, string>; break;
                case BindingType.OnSelect: OnSelectAction -= action as Action<TMP_InputField, string>; break;
                case BindingType.OnDeselect: OnDeselectAction -= action as Action<TMP_InputField, string>; break;
            }
        }
    }
}

