using Causeless3t.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonBinder : DataBinder<Button>, IDataBinder<bool>, IUIEventBinder
    {
        public enum ButtonProperty
        {
            Enable,
            OnClick
        }

        [Serializable]
        public struct BindInfo
        {
            public string Key;
            public ButtonProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfo> _bindInfos = new();
        private Dictionary<string, ButtonProperty> _bindInfoDic;
        private event Action<Button> OnClickAction;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onClick.RemoveListener(OnClick);
            Target.onClick.AddListener(OnClick);
        }

        protected override void OnDestroy()
        {
            Target.onClick.RemoveListener(OnClick);
            base.OnDestroy();
        }
        
        protected override void LoadData()
        {
            _bindInfoDic ??= new Dictionary<string, ButtonProperty>();
            _bindInfoDic.Clear();

            foreach (var info in _bindInfos)
            {
                if (string.IsNullOrEmpty(info.Key))
                    continue;

                if (!_bindInfoDic.TryAdd(info.Key, info.PropertyType))
                {
                    Debug.LogError($"Duplicate binding key '{info.Key}' found in {nameof(ButtonBinder)}.", this);
                }
            }
        }

        public override string[] GetKeyList()
        {
            List<string> result = new();
            _bindInfos.ForEach((info) =>
            {
                if (info.PropertyType == ButtonProperty.OnClick)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        public void SetProperty(string key, bool value)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Button>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case ButtonProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IDataBinder<bool>.GetProperty(string key)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Button>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case ButtonProperty.Enable: return Target.interactable;
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

        public void AddListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case ButtonProperty.OnClick: OnClickAction += action as Action<Button>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            EnsureBindData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case ButtonProperty.OnClick: OnClickAction -= action as Action<Button>; break;
            }
        }
    }
}

