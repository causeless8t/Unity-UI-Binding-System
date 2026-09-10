using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMPTextBinder : ComponentBinder<TMP_Text>,
        IPropertyBinder<string>, IPropertyBinder<Color>, IPropertyBinder<float>
    {
        public enum TMP_TextProperty
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
            public TMP_TextProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, TMP_TextProperty> _bindInfoDic; 

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, string value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_TextProperty.Text: Target.SetText(value); break;
            }
        }

        public void SetProperty(string key, float value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_TextProperty.Alpha: Target.alpha = value; break;
                case TMP_TextProperty.Size: Target.fontSize = value; break;
            }
        }

        public void SetProperty(string key, Color value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case TMP_TextProperty.Color: Target.color = value; break;
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

        Color IPropertyBinder<Color>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_TextProperty.Color: return Target.color;
            }
            return default;
        }

        float IPropertyBinder<float>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_TextProperty.Alpha: return Target.alpha;
                case TMP_TextProperty.Size: return Target.fontSize;
            }
            return default;
        }

        string IPropertyBinder<string>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case TMP_TextProperty.Text: return Target.text;
            }
            return default;
        }
    }
}

