using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IATK
{

    /// <summary>
    /// Bin size drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(IATK.DataMetadata.BinSize))]
    public class BinSizeDrawer : PropertyDrawer 
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = "Index/Bin Count";

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            Rect indexRect = new Rect(position.x, position.y, 30, position.height);
            Rect binCountRect = new Rect(position.x + 35, position.y, position.width - 50, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(indexRect, property.FindPropertyRelative("index"), GUIContent.none);
            EditorGUI.PropertyField(binCountRect, property.FindPropertyRelative("binCount"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

    }

}   // Namespace