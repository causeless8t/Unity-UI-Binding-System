using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class TransformBinder : ComponentBinder<Transform>, IPropertyBinder<Vector3>, IPropertyBinder<Quaternion>
    {
        public enum TransformProperty
        {
            Position,
            Rotation,
            Scale,
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public TransformProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, TransformProperty> _bindInfoDic; 

        protected override void Awake()
        {
            base.Awake();
            Target = transform;
        }
        
        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, Vector3 value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= transform;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TransformProperty.Position: Target.position = value; break;
                case TransformProperty.Scale: Target.localScale = value; break;
            }
        }

        public void SetProperty(string key, Quaternion value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= transform;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TransformProperty.Rotation: Target.rotation = value; break;
            }
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

        Quaternion IPropertyBinder<Quaternion>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= transform;
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TransformProperty.Rotation: return Target.rotation;
            }
            return default;
        }

        Vector3 IPropertyBinder<Vector3>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= transform;
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TransformProperty.Position: return Target.position;
                case TransformProperty.Scale: return Target.localScale;
            }
            return default;
        }
    }
}

