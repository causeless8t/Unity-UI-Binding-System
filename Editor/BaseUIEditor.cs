using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Causeless3t.UI.Editor
{
    [CustomEditor(typeof(BaseUI), true)]
    internal class BaseUIEditor : UnityEditor.Editor
    {
        private bool _showBindings = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            DrawBindingView();
        }

        private void DrawBindingView()
        {
            if (target is not BaseUI baseUI)
                return;

            var bindingMap = BindingViewCollector.Collect(baseUI);

            _showBindings = EditorGUILayout.Foldout(_showBindings, $"UI Bindings ({bindingMap.Count})", true);

            if (!_showBindings)
                return;

            EditorGUI.indentLevel++;

            if (bindingMap.Count == 0)
            {
                EditorGUILayout.HelpBox("No UI bindings found.", MessageType.Info);

                EditorGUI.indentLevel--;
                return;
            }

            foreach (var pair in bindingMap.OrderBy(x => x.Key))
            {
                DrawBindingGroup(pair.Key, pair.Value);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawBindingGroup(string key, List<BindingViewInfo> bindings)
        {
            EditorGUILayout.Space(5);

            DrawKeyHeader(key);

            EditorGUI.indentLevel++;

            foreach (var binding in bindings)
            {
                DrawBinding(binding);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawKeyHeader(string key)
        {
            var rect = EditorGUILayout.GetControlRect();

            EditorGUI.LabelField(rect, key, EditorStyles.boldLabel);
        }

        private static void DrawBinding(BindingViewInfo binding)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(EditorGUI.indentLevel * 8);

            EditorGUILayout.LabelField(GetKindName(binding.Kind), GUILayout.Width(75));

            EditorGUILayout.LabelField(binding.OwnerName, GUILayout.Width(130));

            EditorGUILayout.LabelField(binding.Description, GUILayout.Width(180));

            if (binding.Target != null)
            {
                if (GUILayout.Button("Select", GUILayout.Width(55)))
                {
                    SelectTarget(binding.Target);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static string GetKindName(BindingViewKind kind)
        {
            return kind switch
            {
                BindingViewKind.Getter => "Getter",

                BindingViewKind.Binding => "Binding",

                BindingViewKind.EventHandler => "Handler",

                _ => string.Empty
            };
        }

        private static void SelectTarget(Object target)
        {
            Selection.activeObject = target;

            EditorGUIUtility.PingObject(target);
        }
    }
}