using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class OverrideFlags
{
    [SerializeField] private int selectedFlags;

    public int Flags => selectedFlags;
}

[CustomPropertyDrawer(typeof(OverrideFlags))]
public class MultiSelectExampleDrawer : PropertyDrawer
{
    private readonly string[] options = { "Starting Action", "Override Groups" };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty flagsProp = property.FindPropertyRelative("selectedFlags");

        if (flagsProp != null)
        {
            flagsProp.intValue = EditorGUI.MaskField(position, label, flagsProp.intValue, options);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Expected 'selectedFlags' field not found.");
        }
    }
}
