using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueFlag))]
public class DialogueFlagEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Referință către asset
        DialogueFlag flag = (DialogueFlag)target;

        // Desenează câmpul flagKey
        SerializedProperty flagKeyProp = serializedObject.FindProperty("flagKey");
        EditorGUILayout.PropertyField(flagKeyProp);

        EditorGUILayout.Space();

        // Checkbox real care modifică PlayerPrefs
        bool current = flag.IsTriggered();
        bool newValue = EditorGUILayout.Toggle("Is Triggered (Persistent)", current);

        if (newValue != current)
        {
            if (newValue)
                flag.MarkAsTriggered();
            else
                flag.ResetFlag();
        }

        // Salvează modificările în Inspector
        serializedObject.ApplyModifiedProperties();
    }
}
