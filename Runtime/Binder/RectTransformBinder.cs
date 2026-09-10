using System;
using System.Collections.Generic;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class RectTransformBinder : ComponentBinder<RectTransform>,
        IPropertyBinder<Vector2>, IPropertyBinder<Vector3>, IPropertyBinder<Quaternion>, IPropertyBinder<Rect>
    {
        public enum BindingType
        {
            AnchorPosition,
            Position,
            Rotation,
            Scale,
            Size,
            Rect
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

        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.bindingTypeType, this);
            }
        }

        public void SetProperty(string key, Vector2 value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Size: target.sizeDelta = value; break;
                case BindingType.AnchorPosition: target.anchoredPosition = value; break;
            }
        }

        public void SetProperty(string key, Vector3 value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Position: target.position = value; break;
                case BindingType.Scale: target.localScale = value; break;
            }
        }

        public void SetProperty(string key, Quaternion value)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return;

            var target = GetTarget();
            if (target == null)
                return;
            
            switch (property)
            {
                case BindingType.Rotation: target.rotation = value; break;
            }
        }

        public void SetProperty(string key, Rect value)
        {
            // Only Getter
        }

        Rect IPropertyBinder<Rect>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.Rect: return target.rect;
            }
            return default;
        }

        public override bool HasKey(string key)
        {
            return base.HasKey(key) || _bindingMap.Contains(key);
        }

        Quaternion IPropertyBinder<Quaternion>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.Rotation: return target.rotation;
            }
            return default;
        }

        Vector3 IPropertyBinder<Vector3>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.Position: return target.position;
                case BindingType.Scale: return target.localScale;
            }
            return default;
        }

        Vector2 IPropertyBinder<Vector2>.GetProperty(string key)
        {
            if (!_bindingMap.TryGet(key, out var property))
                return default;

            var target = GetTarget();
            if (target == null)
                return default;
            
            switch (property)
            {
                case BindingType.AnchorPosition: return target.anchoredPosition;
                case BindingType.Size: return target.sizeDelta;
            }
            return default;
        }
    }
}

