// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.AnimatedValues;


namespace TMPro.EditorUtilities
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TMP_InputField), true)]
    public class TMP_InputFieldEditor : SelectableEditor
    {
        private struct m_foldout
        { // Track Inspector foldout panel states, globally.
            public static bool textInput = true;
            public static bool fontSettings = true;
            //public static bool extraSettings = false;
            //public static bool shadowSetting = false;
            //public static bool materialEditor = true;
        }

        private static string[] uiStateLabel = new string[] { "\t- <i>Click to expand</i> -", "\t- <i>Click to collapse</i> -" };
        //private GUIStyle toggleStyle;

        SerializedProperty m_TextViewport;
        SerializedProperty m_TextComponent;
        SerializedProperty m_Text;
        SerializedProperty m_ContentType;
        SerializedProperty m_LineType;
        SerializedProperty m_InputType;
        SerializedProperty m_CharacterValidation;
        SerializedProperty m_KeyboardType;
        SerializedProperty m_CharacterLimit;
        SerializedProperty m_CaretBlinkRate;
        SerializedProperty m_CaretWidth;
        SerializedProperty m_CaretColor;
        SerializedProperty m_CustomCaretColor;
        SerializedProperty m_SelectionColor;
        SerializedProperty m_HideMobileInput;
        SerializedProperty m_Placeholder;
        SerializedProperty m_OnValueChanged;
        SerializedProperty m_OnEndEdit;
        SerializedProperty m_OnFocusLost;
        SerializedProperty m_ReadOnly;
        SerializedProperty m_RichText;

        AnimBool m_CustomColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_TextViewport = serializedObject.FindProperty("m_TextViewport");
            m_TextComponent = serializedObject.FindProperty("m_TextComponent");
            m_Text = serializedObject.FindProperty("m_Text");
            m_ContentType = serializedObject.FindProperty("m_ContentType");
            m_LineType = serializedObject.FindProperty("m_LineType");
            m_InputType = serializedObject.FindProperty("m_InputType");
            m_CharacterValidation = serializedObject.FindProperty("m_CharacterValidation");
            m_KeyboardType = serializedObject.FindProperty("m_KeyboardType");
            m_CharacterLimit = serializedObject.FindProperty("m_CharacterLimit");
            m_CaretBlinkRate = serializedObject.FindProperty("m_CaretBlinkRate");
            m_CaretWidth = serializedObject.FindProperty("m_CaretWidth");
            m_CaretColor = serializedObject.FindProperty("m_CaretColor");
            m_CustomCaretColor = serializedObject.FindProperty("m_CustomCaretColor");
            m_SelectionColor = serializedObject.FindProperty("m_SelectionColor");
            m_HideMobileInput = serializedObject.FindProperty("m_HideMobileInput");
            m_Placeholder = serializedObject.FindProperty("m_Placeholder");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            m_OnEndEdit = serializedObject.FindProperty("m_OnEndEdit");
            m_OnFocusLost = serializedObject.FindProperty("m_OnFocusLost");
            m_ReadOnly = serializedObject.FindProperty("m_ReadOnly");
            m_RichText = serializedObject.FindProperty("m_RichText");

            m_CustomColor = new AnimBool(m_CustomCaretColor.boolValue);
            m_CustomColor.valueChanged.AddListener(Repaint);

            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_CustomColor.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_TextViewport);

            EditorGUILayout.PropertyField(m_TextComponent);

            TextMeshProUGUI text = null;
            if (m_TextComponent != null && m_TextComponent.objectReferenceValue != null)
            {
                text = m_TextComponent.objectReferenceValue as TextMeshProUGUI;
                //if (text.supportRichText)
                //{
                //    EditorGUILayout.HelpBox("Using Rich Text with input is unsupported.", MessageType.Warning);
                //}
            }

            EditorGUI.BeginDisabledGroup(m_TextComponent == null || m_TextComponent.objectReferenceValue == null);

            // TEXT INPUT BOX
            Rect rect = EditorGUILayout.GetControlRect(false, 25);
            EditorGUIUtility.labelWidth = 130f;
            //EditorGUIUtility.fieldWidth;

            rect.y += 2;
            GUI.Label(rect, "<b>TEXT INPUT BOX</b>" + (m_foldout.textInput ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label);
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width - 150, rect.height), GUIContent.none, GUI.skin.label))
                m_foldout.textInput = !m_foldout.textInput;

            // Toggle showing Rich Tags
            //GUI.Label(new Rect(rect.width - 125, rect.y + 4, 125, 24), "<i>Enable RTL Editor</i>", toggleStyle);

            if (m_foldout.textInput)
            {
                EditorGUI.BeginChangeCheck();
                m_Text.stringValue = EditorGUILayout.TextArea(m_Text.stringValue, TMP_UIStyleManager.TextAreaBoxEditor, GUILayout.Height(125), GUILayout.ExpandWidth(true));
            }


            // INPUT FIELD SETTINGS
            if (GUILayout.Button("<b>INPUT FIELD SETTINGS</b>" + (m_foldout.fontSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
                m_foldout.fontSettings = !m_foldout.fontSettings;


            if (m_foldout.fontSettings)
            {

                EditorGUILayout.PropertyField(m_CharacterLimit);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_ContentType);
                if (!m_ContentType.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;

                    if (m_ContentType.enumValueIndex == (int)TMP_InputField.ContentType.Standard ||
                        m_ContentType.enumValueIndex == (int)TMP_InputField.ContentType.Autocorrected ||
                        m_ContentType.enumValueIndex == (int)TMP_InputField.ContentType.Custom)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(m_LineType);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (text != null)
                            {
                                if (m_LineType.enumValueIndex == (int)TMP_InputField.LineType.SingleLine)
                                    text.enableWordWrapping = false;
                                else
                                    text.enableWordWrapping = true;
                            }
                        }
                    }

                    if (m_ContentType.enumValueIndex == (int)TMP_InputField.ContentType.Custom)
                    {
                        EditorGUILayout.PropertyField(m_InputType);
                        EditorGUILayout.PropertyField(m_KeyboardType);
                        EditorGUILayout.PropertyField(m_CharacterValidation);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_Placeholder);
                EditorGUILayout.PropertyField(m_CaretBlinkRate);
                EditorGUILayout.PropertyField(m_CaretWidth);

                EditorGUILayout.PropertyField(m_CustomCaretColor);

                m_CustomColor.target = m_CustomCaretColor.boolValue;

                if (EditorGUILayout.BeginFadeGroup(m_CustomColor.faded))
                {
                    EditorGUILayout.PropertyField(m_CaretColor);
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(m_SelectionColor);
                EditorGUILayout.PropertyField(m_HideMobileInput);
                EditorGUILayout.PropertyField(m_ReadOnly);
                EditorGUILayout.PropertyField(m_RichText);

            }
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_OnValueChanged);
            EditorGUILayout.PropertyField(m_OnEndEdit);
            EditorGUILayout.PropertyField(m_OnFocusLost);

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
