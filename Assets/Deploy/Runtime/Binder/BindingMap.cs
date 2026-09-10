using System.Collections.Generic;
using UnityEngine;

namespace Causeless3t.UI
{
    internal sealed class BindingMap<TProperty>
    {
        private readonly Dictionary<string, TProperty> _bindings = new();

        public void Clear()
        {
            _bindings.Clear();
        }

        public bool Add(string key, TProperty property, Object context = null)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (!_bindings.TryAdd(key, property))
            {
                Debug.LogError($"Duplicate binding key '{key}'.", context);

                return false;
            }
            
            return true;
        }

        public bool Contains(string key)
        {
            return !string.IsNullOrEmpty(key) && _bindings.ContainsKey(key);
        }

        public bool TryGet(string key, out TProperty property)
        {
            if (string.IsNullOrEmpty(key))
            {
                property = default;
                return false;
            }

            return _bindings.TryGetValue(key, out property);
        }
    }
}