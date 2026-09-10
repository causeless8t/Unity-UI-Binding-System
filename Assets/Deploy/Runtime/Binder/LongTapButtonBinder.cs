using System;
using System.Collections.Generic;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(LongTapButton))]
    public sealed class LongTapButtonBinder : ComponentBinder<LongTapButton>, IPropertyBinder<bool>, IEventBinder
    {
        public enum BindingType
        {
            Enable,
            OnClick,
            OnLongTap,
            OnVeryLongTap
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
        private event Action<LongTapButton> OnClickAction;
        private event Action<LongTapButton> OnLongTapAction;
        private event Action<LongTapButton> OnVeryLongTapAction;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onClick.RemoveListener(OnClick);
            Target.onLongTap.RemoveListener(OnLongTap);
            Target.onVeryLongTap.RemoveListener(OnVeryLongTap);
            Target.onClick.AddListener(OnClick);
            Target.onLongTap.AddListener(OnLongTap);
            Target.onVeryLongTap.AddListener(OnVeryLongTap);
        }

        protected override void OnDestroy()
        {
            Target.onClick.RemoveListener(OnClick);
            Target.onLongTap.RemoveListener(OnLongTap);
            Target.onVeryLongTap.RemoveListener(OnVeryLongTap);
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

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }
        
        private void OnClick()
        {
            OnClickAction?.Invoke(Target);
        }
        
        private void OnLongTap()
        {
            OnLongTapAction?.Invoke(Target);
        }
        
        private void OnVeryLongTap()
        {
            OnVeryLongTapAction?.Invoke(Target);
        }

        public void AddListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            switch (property)
            {
                case BindingType.OnClick: OnClickAction += action as Action<LongTapButton>; break;
                case BindingType.OnLongTap: OnLongTapAction += action as Action<LongTapButton>; break;
                case BindingType.OnVeryLongTap: OnVeryLongTapAction += action as Action<LongTapButton>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            switch (property)
            {
                case BindingType.OnClick: OnClickAction -= action as Action<LongTapButton>; break;
                case BindingType.OnLongTap: OnLongTapAction -= action as Action<LongTapButton>; break;
                case BindingType.OnVeryLongTap: OnVeryLongTapAction -= action as Action<LongTapButton>; break;
            }
        }
    }
}

