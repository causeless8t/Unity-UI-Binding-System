using System;
using System.Collections.Generic;
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

                if (!TryValidate(type, method, attribute, out var key))
                {
                    continue;
                }

                bindings.Add(new UIEventBindingInfo(key, attribute.DelegateType, method));
            }

            return bindings.ToArray();
        }

        private static bool TryValidate(Type targetType, MethodInfo method, UIRegisterAttribute attribute, out string key)
        {
            key = string.IsNullOrEmpty(attribute.Key) ? method.Name : attribute.Key;

            if (attribute.DelegateType == null)
            {
                Debug.LogError(
                    $"{FormatMethod(targetType, method)}: " +
                    $"{nameof(UIRegisterAttribute)} DelegateType is null.");

                return false;
            }

            if (!typeof(Delegate).IsAssignableFrom(attribute.DelegateType))
            {
                Debug.LogError(
                    $"{FormatMethod(targetType, method)}: " +
                    $"{attribute.DelegateType.Name} is not a delegate type.");

                return false;
            }

            var invokeMethod = attribute.DelegateType.GetMethod("Invoke");

            if (invokeMethod == null)
            {
                Debug.LogError(
                    $"{FormatMethod(targetType, method)}: " +
                    $"Delegate Invoke method could not be found.");

                return false;
            }

            if (invokeMethod.ReturnType != method.ReturnType)
            {
                LogSignatureMismatch(
                    targetType,
                    method,
                    attribute.DelegateType);

                return false;
            }

            var delegateParameters = invokeMethod.GetParameters();
            var methodParameters = method.GetParameters();

            if (delegateParameters.Length != methodParameters.Length)
            {
                LogSignatureMismatch(
                    targetType,
                    method,
                    attribute.DelegateType);

                return false;
            }

            for (var i = 0; i < delegateParameters.Length; i++)
            {
                if (delegateParameters[i].ParameterType ==
                    methodParameters[i].ParameterType)
                {
                    continue;
                }

                LogSignatureMismatch(
                    targetType,
                    method,
                    attribute.DelegateType);

                return false;
            }

            return true;
        }
        
        private static void LogSignatureMismatch(Type targetType, MethodInfo method, Type delegateType)
        {
            Debug.LogError(
                $"{FormatMethod(targetType, method)}: " +
                $"method signature does not match delegate " +
                $"'{delegateType.Name}'.");
        }

        private static string FormatMethod(Type targetType, MethodInfo method)
        {
            return $"{targetType.FullName}.{method.Name}";
        }

        public static void Clear()
        {
            _bindings.Clear();
        }
    }
}