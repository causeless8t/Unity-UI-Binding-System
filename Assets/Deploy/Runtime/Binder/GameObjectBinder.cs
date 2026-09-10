using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class GameObjectBinder : ComponentBinder<GameObject>, IPropertyBinder<bool>, ICommandBinder<bool>
    {
        public enum GameObjectProperty
        {
            IsActive,
            SetActive
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public GameObjectProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, GameObjectProperty> _bindInfoDic; 

        protected override void Awake()
        {
            base.Awake();
            Target = gameObject;
        }

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }
        
        public void SetProperty(string key, bool value)
        {
            // Only Getter
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= gameObject;
            if (Target.IsUnityNull()) return default;
            switch (type)
            { 
                case GameObjectProperty.IsActive: return Target.activeSelf;
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
                LoadData();
        }

        public void InvokeMethod(string key, bool param)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= gameObject;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case GameObjectProperty.SetActive: Target.SetActive(param); break;
            }
        }
    }
}

