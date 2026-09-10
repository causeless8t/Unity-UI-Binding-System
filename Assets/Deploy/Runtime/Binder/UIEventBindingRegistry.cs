using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Causeless3t.UI
{
    internal readonly struct UIEventBindingInfo
    {
        public string Key { get; }
        public Type DelegateType { get; }
        public MethodInfo Method { get; }

        public UIEventBindingInfo(
            string key,
            Type delegateType,
            MethodInfo method)
        {
            Key = key;
            DelegateType = delegateType;
            Method = method;
        }
    }
    
    internal static class UIEventBindingRegistry
    {
        private static readonly Dictionary<Type, List<UIEventBindingInfo>> _bindings = new();

        private static bool _initialized;

        public static IReadOnlyList<UIEventBindingInfo> GetBindings(Type type)
        {
            EnsureInitialized();

            return _bindings.TryGetValue(type, out var bindings)
                ? bindings
                : Array.Empty<UIEventBindingInfo>();
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            Initialize();
        }

        private static void Initialize()
        {
            // 현재 InitUIEventBindInfo 내용 이동
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute(typeof(UIRegisterAttribute), true) != null);
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<UIRegisterAttribute>();
                    if (attribute.DelegateType == null)
                    {
                        Debug.LogError("AutoRegistUIEvents Failed. actionType is error");
                        return;
                    }
            
                    var key = attribute.Key;
                    if (string.IsNullOrEmpty(key))
                    {
                        key = method.Name;
                    }
            
                    if (!_bindings.TryGetValue(type, out var list))
                    {
                        list = new();
                        _bindings.Add(type, list);
                    }
                    
                    list.Add(new(key, attribute.DelegateType, method));
                }
            }
        }
    }
}