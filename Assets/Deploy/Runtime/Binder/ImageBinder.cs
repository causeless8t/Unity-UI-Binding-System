using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class ImageBinder : ComponentBinder<Image>, IPropertyBinder<Sprite>, IPropertyBinder<float>, IPropertyBinder<Color>
    {
        public enum BindingType
        {
            Sprite,
            FillAmount,
            Color
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

        public void SetProperty(string key, Sprite value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.Sprite)
                return;
            
            var target = GetTarget();
            if (target == null)
                return;

            target.sprite = value;
        }

        public void SetProperty(string key, float value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            if (property != BindingType.FillAmount)
                return;
            
            var target = GetTarget();
            if (target == null)
                return;

            target.fillAmount = value;
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

        Color IPropertyBinder<Color>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            return property == BindingType.Color ? target.color : default;
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
            
            return property == BindingType.FillAmount ? target.fillAmount : default;
        }

        Sprite IPropertyBinder<Sprite>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;
            
            var target = GetTarget();
            if (target == null)
                return default;

            return property == BindingType.Sprite ? target.sprite : default;
        }
    }
}

