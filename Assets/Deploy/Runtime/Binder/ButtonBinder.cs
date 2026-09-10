using Causeless3t.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonBinder : ComponentBinder<Button>, IPropertyBinder<bool>, IEventBinder
    {
        public enum ButtonProperty
        {
            Enable,
            OnClick
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public ButtonProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private BindingMap<ButtonProperty> _bindingMap;
        private event Action<Button> OnClickAction;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onClick.RemoveListener(OnClick);
            Target.onClick.AddListener(OnClick);
        }

        protected override void OnDestroy()
        {
            Target.onClick.RemoveListener(OnClick);
            base.OnDestroy();
        }
        
        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.PropertyType, this);
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
                case ButtonProperty.Enable:
                    target.interactable = value;
                    break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();

            if (target == null)
                return default;

            return property switch
            {
                ButtonProperty.Enable => target.interactable,
                _ => default
            };
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        private void OnClick()
        {
            OnClickAction?.Invoke(Target);
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != ButtonProperty.OnClick)
                return;

            if (action is Action<Button> callback)
                OnClickAction += callback;
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != ButtonProperty.OnClick)
                return;

            if (action is Action<Button> callback)
                OnClickAction -= callback;
        }
    }
}

