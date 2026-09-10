using System;
using System.Collections.Generic;
using System.Linq;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(LongTapButton))]
    public sealed class LongTapButtonBinder : ComponentBinder<LongTapButton>, IPropertyBinder<bool>, IEventBinder
    {
        public enum LongTapButtonProperty
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
            public LongTapButtonProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, LongTapButtonProperty> _bindInfoDic; 
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

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }
        
        public override string[] GetKeyList()
        {
            List<string> result = new();
            _bindInfos.ForEach((info) =>
            {
                if (info.PropertyType != LongTapButtonProperty.Enable)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }
        
        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<LongTapButton>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case LongTapButtonProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IPropertyBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<LongTapButton>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case LongTapButtonProperty.Enable: return Target.interactable;
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
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case LongTapButtonProperty.OnClick: OnClickAction += action as Action<LongTapButton>; break;
                case LongTapButtonProperty.OnLongTap: OnLongTapAction += action as Action<LongTapButton>; break;
                case LongTapButtonProperty.OnVeryLongTap: OnVeryLongTapAction += action as Action<LongTapButton>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case LongTapButtonProperty.OnClick: OnClickAction -= action as Action<LongTapButton>; break;
                case LongTapButtonProperty.OnLongTap: OnLongTapAction -= action as Action<LongTapButton>; break;
                case LongTapButtonProperty.OnVeryLongTap: OnVeryLongTapAction -= action as Action<LongTapButton>; break;
            }
        }
    }
}

