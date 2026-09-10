using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMPTextBinder : ComponentBinder<TMP_Text>,
        IPropertyBinder<string>, IPropertyBinder<Color>, IPropertyBinder<float>
    {
        public enum BindingType
        {
            Text,
            Color,
            Alpha,
            Size
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

        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.bindingTypeType, this);
            }
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

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Alpha: target.alpha = value; break;
                case BindingType.Size: target.fontSize = value; break;
            }
        }

        public void SetProperty(string key, Color value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.Color)
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            target.color = value;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        Color IPropertyBinder<Color>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.Color ? target.color : default;
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
                case BindingType.Alpha: return target.alpha;
                case BindingType.Size: return target.fontSize;
            }
            return default;
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
    }
}

