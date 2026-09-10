#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace Causeless3t.UI.Editor
{
    internal readonly struct ValidationMessage
    {
        public string Message { get; }
        public MessageType Type { get; }

        public ValidationMessage(string message, MessageType type)
        {
            Message = message;
            Type = type;
        }
    }
    
    internal static class BinderValidator
    {
        public static List<ValidationMessage> Validate(
            SerializedObject serializedObject,
            SerializedProperty bindings)
        {
            var errors = new List<ValidationMessage>();

            ValidateBindingKeys(bindings, errors);

            ValidateGetterKey(serializedObject, bindings, errors);

            return errors;
        }

        private static void ValidateBindingKeys(
            SerializedProperty bindings,
            List<ValidationMessage> errors)
        {
            if (bindings == null || !bindings.isArray)
                return;

            var keys = new HashSet<string>();

            for (var i = 0; i < bindings.arraySize; i++)
            {
                var element =
                    bindings.GetArrayElementAtIndex(i);

                var keyProperty =
                    element.FindPropertyRelative("Key");

                if (keyProperty == null)
                    continue;

                var key = keyProperty.stringValue;

                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add(new ValidationMessage(
                        $"Binding #{i + 1}: Key is empty.",
                        MessageType.Warning));

                    continue;
                }

                if (!keys.Add(key))
                {
                    errors.Add(new ValidationMessage(
                        $"Duplicate binding key '{key}'.",
                        MessageType.Error));
                }
            }
        }

        private static void ValidateGetterKey(
            SerializedObject serializedObject,
            SerializedProperty bindings,
            List<ValidationMessage> errors)
        {
            if (bindings == null || !bindings.isArray)
                return;

            var getterKey =
                serializedObject.FindProperty("getterKey");

            if (getterKey == null ||
                string.IsNullOrEmpty(getterKey.stringValue))
            {
                return;
            }

            for (var i = 0; i < bindings.arraySize; i++)
            {
                var element =
                    bindings.GetArrayElementAtIndex(i);

                var keyProperty =
                    element.FindPropertyRelative("Key");

                if (keyProperty == null)
                    continue;

                if (keyProperty.stringValue !=
                    getterKey.stringValue)
                {
                    continue;
                }

                errors.Add(new ValidationMessage(
                    $"Getter key '{getterKey.stringValue}' is also used as a binding key.",
                    MessageType.Error));

                return;
            }
        }
    }
}

#endif