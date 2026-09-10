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
        private static readonly Dictionary<Type, IReadOnlyList<UIEventBindingInfo>> _bindings = new();

        public static IReadOnlyList<UIEventBindingInfo> GetBindings(Type type)
        {
            if (type == null)
                return Array.Empty<UIEventBindingInfo>();

            if (_bindings.TryGetValue(type, out var bindings))
                return bindings;

            bindings = CreateBindings(type);
            _bindings.Add(type, bindings);

            return bindings;
        }

        private static IReadOnlyList<UIEventBindingInfo> CreateBindings(Type type)
        {
            var bindings = new List<UIEventBindingInfo>();

            var methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<UIRegisterAttribute>(true);

                if (attribute == null)
                    continue;

                if (attribute.DelegateType == null)
                {
                    Debug.LogError(
                        $"[{nameof(UIRegisterAttribute)}] DelegateType is null: " +
                        $"{type.FullName}.{method.Name}");

                    continue;
                }

                if (!IsCompatible(method, attribute.DelegateType))
                {
                    Debug.LogError(
                        $"[{nameof(UIRegisterAttribute)}] Delegate signature mismatch: " +
                        $"{type.FullName}.{method.Name} / " +
                        $"{attribute.DelegateType.FullName}");

                    continue;
                }

                var key = string.IsNullOrEmpty(attribute.Key)
                    ? method.Name
                    : attribute.Key;

                bindings.Add(new UIEventBindingInfo(key, attribute.DelegateType, method));
            }

            return bindings;
        }

        private static bool IsCompatible(MethodInfo method, Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                return false;

            var invokeMethod = delegateType.GetMethod("Invoke");

            if (invokeMethod == null)
                return false;

            if (invokeMethod.ReturnType != method.ReturnType)
                return false;

            var delegateParameters = invokeMethod.GetParameters();
            var methodParameters = method.GetParameters();

            if (delegateParameters.Length != methodParameters.Length)
                return false;

            for (var i = 0; i < delegateParameters.Length; i++)
            {
                if (delegateParameters[i].ParameterType != methodParameters[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        public static void Clear()
        {
            _bindings.Clear();
        }
    }
}