using UnityEditor;

namespace Causeless3t.UI.Editor
{
    [CustomEditor(typeof(BinderBase), true)]
    public class BinderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidation()
        {
            var bindInfos = serializedObject.FindProperty("_bindInfos");

            if (bindInfos == null)
                return;

            var messages = BinderValidator.Validate(serializedObject, bindInfos);

            foreach (var message in messages)
            {
                EditorGUILayout.HelpBox(message.Message, message.Type);
            }
        }
    }
}