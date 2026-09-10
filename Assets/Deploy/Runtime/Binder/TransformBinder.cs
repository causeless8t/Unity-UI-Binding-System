using System;
using System.Collections.Generic;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class TransformBinder : ComponentBinder<Transform>, IPropertyBinder<Vector3>, IPropertyBinder<Quaternion>
    {
        public enum BindingType
        {
            Position,
            Rotation,
            Scale,
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
            Target = transform;
        }
        
        protected override void BuildBindings()
        {
            _bindingMap.Clear();

            foreach (var info in _bindInfos)
            {
                _bindingMap.Add(info.Key, info.bindingTypeType, this);
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

            if (property != BindingType.Rotation)
                return;
            
            var target = GetTarget();
            if (target == null)
                return;

            target.rotation = value;
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

            return property == BindingType.Rotation ? target.rotation : default;
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
                case BindingType.Position: return Target.position;
                case BindingType.Scale: return Target.localScale;
            }
            return default;
        }
    }
}

