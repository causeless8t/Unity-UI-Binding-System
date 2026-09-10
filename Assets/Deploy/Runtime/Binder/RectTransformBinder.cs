using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class RectTransformBinder : ComponentBinder<RectTransform>,
        IPropertyBinder<Vector2>, IPropertyBinder<Vector3>, IPropertyBinder<Quaternion>, IPropertyBinder<Rect>
    {
        public enum RectTransformProperty
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
            public RectTransformProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, RectTransformProperty> _bindInfoDic; 

        protected override void BuildBindings()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, Vector2 value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case RectTransformProperty.Size: Target.sizeDelta = value; break;
                case RectTransformProperty.AnchorPosition: Target.anchoredPosition = value; break;
            }
        }

        public void SetProperty(string key, Vector3 value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case RectTransformProperty.Position: Target.position = value; break;
                case RectTransformProperty.Scale: Target.localScale = value; break;
            }
        }

        public void SetProperty(string key, Quaternion value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case RectTransformProperty.Rotation: Target.rotation = value; break;
            }
        }

        public void SetProperty(string key, Rect value)
        {
            // Only Getter
        }

        Rect IPropertyBinder<Rect>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case RectTransformProperty.Rect: return Target.rect;
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
                BuildBindings();
        }

        Quaternion IPropertyBinder<Quaternion>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case RectTransformProperty.Rotation: return Target.rotation;
            }
            return default;
        }

        Vector3 IPropertyBinder<Vector3>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case RectTransformProperty.Position: return Target.position;
                case RectTransformProperty.Scale: return Target.localScale;
            }
            return default;
        }

        Vector2 IPropertyBinder<Vector2>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case RectTransformProperty.AnchorPosition: return Target.anchoredPosition;
                case RectTransformProperty.Size: return Target.sizeDelta;
            }
            return default;
        }
    }
}

