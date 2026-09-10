using System;
using System.Collections.Generic;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class GameObjectBinder : ComponentBinder<GameObject>, IPropertyBinder<bool>, ICommandBinder<bool>
    {
        public enum BindingType
        {
            IsActive,
            SetActive
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

        protected override void Awake()
        {
            base.Awake();
            Target = gameObject;
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
            // Only Getter
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return false;

            return property == BindingType.IsActive && GetTarget().activeSelf;
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
                case BindingType.SetActive:
                    target.SetActive(param);
                    break;
            }
        }
    }
}

