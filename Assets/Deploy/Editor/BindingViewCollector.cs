using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Causeless3t.UI.Editor
{
    internal static class BindingViewCollector
    {
        public static IReadOnlyDictionary<string, List<BindingViewInfo>> Collect(
            BaseUI baseUI)
        {
            var result = new Dictionary<string, List<BindingViewInfo>>();

            if (baseUI == null)
                return result;

            CollectBinders(baseUI, result);
            CollectEventHandlers(baseUI, result);

            return result;
        }

        private static void CollectBinders(BaseUI baseUI, Dictionary<string, List<BindingViewInfo>> result)
        {
            var behaviours = baseUI.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour is not IBinder)
                    continue;

                if (!BelongsTo(baseUI, behaviour))
                    continue;

                CollectBinder(behaviour, result);
            }
        }

        private static void CollectBinder(MonoBehaviour binder, Dictionary<string, List<BindingViewInfo>> result)
        {
            var serializedObject = new SerializedObject(binder);

            serializedObject.Update();

            CollectGetter(binder, serializedObject, result);

            CollectBindings(binder, serializedObject, result);
        }

        private static void CollectGetter(
            MonoBehaviour binder,
            SerializedObject serializedObject,
            Dictionary<string, List<BindingViewInfo>> result)
        {
            var getterKey = serializedObject.FindProperty("getterKey");

            if (getterKey == null)
                return;

            var key = getterKey.stringValue;

            if (string.IsNullOrWhiteSpace(key))
                return;

            Add(result,
                new BindingViewInfo(
                    key,
                    binder.GetType().Name,
                    "Getter",
                    BindingViewKind.Getter,
                    binder));
        }

        private static void CollectBindings(
            MonoBehaviour binder,
            SerializedObject serializedObject,
            Dictionary<string, List<BindingViewInfo>> result)
        {
            var bindings =
                serializedObject.FindProperty("_bindings");

            if (bindings == null || !bindings.isArray)
            {
                return;
            }

            for (var i = 0; i < bindings.arraySize; i++)
            {
                var element = bindings.GetArrayElementAtIndex(i);

                var keyProperty = element.FindPropertyRelative("Key");

                var typeProperty = element.FindPropertyRelative("Type");

                if (keyProperty == null)
                    continue;

                var key = keyProperty.stringValue;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                Add(result,
                    new BindingViewInfo(
                        key,
                        binder.GetType().Name,
                        GetBindingTypeName(typeProperty),
                        BindingViewKind.Binding,
                        binder));
            }
        }

        private static void CollectEventHandlers(BaseUI baseUI, Dictionary<string, List<BindingViewInfo>> result)
        {
            var type = baseUI.GetType();

            var methods = type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<UIRegisterAttribute>(true);

                if (attribute == null)
                    continue;

                var key = string.IsNullOrWhiteSpace(attribute.Key) ? method.Name : attribute.Key;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                Add(result,
                    new BindingViewInfo(
                        key,
                        type.Name,
                        GetMethodDescription(method),
                        BindingViewKind.EventHandler,
                        baseUI));
            }
        }

        private static bool BelongsTo(BaseUI baseUI, Component binder)
        {
            var manager = binder.GetComponentInParent<BaseUI>(true);
            return manager == baseUI;
        }

        private static string GetBindingTypeName(SerializedProperty property)
        {
            if (property == null)
                return "Unknown";

            if (property.propertyType != SerializedPropertyType.Enum)
            {
                return property.displayName;
            }

            var index = property.enumValueIndex;

            if (index < 0 || index >= property.enumDisplayNames.Length)
            {
                return "Unknown";
            }

            return property.enumDisplayNames[index];
        }

        private static string GetMethodDescription(MethodInfo method)
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
                return $"{method.Name}()";

            var parameterTypes = new string[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = GetFriendlyTypeName(parameters[i].ParameterType);
            }

            return $"{method.Name}({string.Join(", ", parameterTypes)})";
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;

            var index = name.IndexOf('`');

            if (index >= 0) 
                name = name[..index];

            var arguments = type.GetGenericArguments();

            var argumentNames = new string[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                argumentNames[i] = GetFriendlyTypeName(arguments[i]);
            }

            return $"{name}<{string.Join(", ", argumentNames)}>";
        }

        private static void Add(Dictionary<string, List<BindingViewInfo>> result, BindingViewInfo info)
        {
            if (!result.TryGetValue(info.Key, out var list))
            {
                list = new List<BindingViewInfo>();

                result.Add(info.Key, list);
            }

            list.Add(info);
        }
    }
}