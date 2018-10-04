// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TMP_SpriteAsset))]
    public class TMP_SpriteAssetEditor : Editor
    {
        private struct UI_PanelState
        {
            public static bool spriteAssetInfoPanel = true;
            public static bool spriteInfoPanel = false;
        }

        private int m_selectedElement = -1;
        private int m_page = 0;

        private const string k_UndoRedo = "UndoRedoPerformed";

        private string m_searchPattern;
        private List<int> m_searchList;
        private bool m_isSearchDirty;

        private SerializedProperty m_spriteAtlas_prop;
        private SerializedProperty m_material_prop;
        private SerializedProperty m_spriteInfoList_prop;

        private bool isAssetDirty = false;
      
        private string[] uiStateLabel = new string[] { "<i>(Click to expand)</i>", "<i>(Click to collapse)</i>" };

        private float m_xOffset;
        private float m_yOffset;
        private float m_xAdvance;
        private float m_scale;


        public void OnEnable()
        {
            m_spriteAtlas_prop = serializedObject.FindProperty("spriteSheet");
            m_material_prop = serializedObject.FindProperty("material");
            m_spriteInfoList_prop = serializedObject.FindProperty("spriteInfoList");


            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();
        
        }


        public override void OnInspectorGUI()
        {

            //Debug.Log("OnInspectorGUI Called.");
            Event currentEvent = Event.current;
            string evt_cmd = currentEvent.commandName; // Get Current Event CommandName to check for Undo Events

            serializedObject.Update();

            EditorGUIUtility.labelWidth = 135;

            // HEADER
            GUILayout.Label("<b>TextMeshPro - Sprite Asset</b>", TMP_UIStyleManager.Section_Label);


            // TEXTMESHPRO SPRITE INFO PANEL
            GUILayout.Label("Sprite Info", TMP_UIStyleManager.Section_Label);
            EditorGUI.indentLevel = 1;

            //GUI.enabled = false; // Lock UI
      
            EditorGUILayout.PropertyField(m_spriteAtlas_prop , new GUIContent("Sprite Atlas"));
            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_material_prop, new GUIContent("Default Material"));

            // SPRITE LIST
            GUI.enabled = true; // Unlock UI 
            GUILayout.Space(10);
            EditorGUI.indentLevel = 0;


            if (GUILayout.Button("Sprite List\t\t" + (UI_PanelState.spriteInfoPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
                UI_PanelState.spriteInfoPanel = !UI_PanelState.spriteInfoPanel;

            if (UI_PanelState.spriteInfoPanel)
            {
                int arraySize = m_spriteInfoList_prop.arraySize;
                int itemsPerPage = 10; // (Screen.height - 292) / 80;

                // Display Glyph Management Tools
                EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label, GUILayout.ExpandWidth(true));
                {
                    // Search Bar implementation
                    #region DISPLAY SEARCH BAR
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 110f;
                        EditorGUI.BeginChangeCheck();
                        string searchPattern = EditorGUILayout.TextField("Sprite Search", m_searchPattern, "SearchTextField");
                        if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
                        {
                            if (string.IsNullOrEmpty(searchPattern) == false)
                            {
                                //GUIUtility.keyboardControl = 0;
                                m_searchPattern = searchPattern.ToLower(System.Globalization.CultureInfo.InvariantCulture).Trim();

                                // Search Glyph Table for potential matches
                                SearchGlyphTable(m_searchPattern, ref m_searchList);
                            }

                            m_isSearchDirty = false;
                        }

                        string styleName = string.IsNullOrEmpty(m_searchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
                        if (GUILayout.Button(GUIContent.none, styleName))
                        {
                            GUIUtility.keyboardControl = 0;
                            m_searchPattern = string.Empty;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    // Display Page Navigation
                    if (!string.IsNullOrEmpty(m_searchPattern))
                        arraySize = m_searchList.Count;

                    // Display Page Navigation
                    DisplayGlyphPageNavigation(arraySize, itemsPerPage);
                }
                EditorGUILayout.EndVertical();

                if (arraySize > 0)
                {
                    // Display each SpriteInfo entry using the SpriteInfo property drawer.
                    for (int i = itemsPerPage * m_page; i < arraySize && i < itemsPerPage * (m_page + 1); i++)
                    {
                        // Define the start of the selection region of the element.
                        Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        int elementIndex = i;
                        if (!string.IsNullOrEmpty(m_searchPattern))
                            elementIndex = m_searchList[i];

                        SerializedProperty spriteInfo = m_spriteInfoList_prop.GetArrayElementAtIndex(elementIndex);

                        EditorGUI.BeginDisabledGroup(i != m_selectedElement);
                        {
                            EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label, GUILayout.Height(60));
                            {
                                EditorGUILayout.PropertyField(spriteInfo);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUI.EndDisabledGroup();

                        // Define the end of the selection region of the element.
                        Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        // Check for Item selection
                        Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                        if (DoSelectionCheck(selectionArea))
                        {
                            m_selectedElement = i;
                            GUIUtility.keyboardControl = 0;

                        }

                        // Draw & Handle Section Area
                        if (m_selectedElement == i)
                        {
                            // Draw selection highlight
                            TMP_EditorUtility.DrawBox(selectionArea, 2f, new Color32(40, 192, 255, 255));

                            // Draw options to Add or Remove Sprites
                            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
                            controlRect.width /= 8;

                            // Add new Sprite
                            controlRect.x += controlRect.width * 6;
                            if (GUI.Button(controlRect, "+"))
                            {
                                m_spriteInfoList_prop.arraySize += 1;

                                int index = m_spriteInfoList_prop.arraySize - 1;

                                SerializedProperty spriteInfo_prop = m_spriteInfoList_prop.GetArrayElementAtIndex(index);

                                // Copy properties of the selected element
                                CopySerializedProperty(m_spriteInfoList_prop.GetArrayElementAtIndex(elementIndex), ref spriteInfo_prop);

                                spriteInfo_prop.FindPropertyRelative("id").intValue = index;
                                serializedObject.ApplyModifiedProperties();

                                m_isSearchDirty = true;
                            }

                            // Delete selected Sprite
                            controlRect.x += controlRect.width;
                            if (m_selectedElement == -1) GUI.enabled = false;
                            if (GUI.Button(controlRect, "-"))
                            {
                                m_spriteInfoList_prop.DeleteArrayElementAtIndex(elementIndex);

                                m_selectedElement = -1;
                                serializedObject.ApplyModifiedProperties();

                                m_isSearchDirty = true;

                                return;
                            }


                        }
                    }
                }

                DisplayGlyphPageNavigation(arraySize, itemsPerPage);

                EditorGUIUtility.labelWidth = 40f;
                EditorGUIUtility.fieldWidth = 20f;

                GUILayout.Space(5f);

                // GLOBAL TOOLS
                #region Global Tools
                GUI.enabled = true;
                EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label);
                Rect rect = EditorGUILayout.GetControlRect(false, 40);
               
                float width = (rect.width - 75f) / 4;
                EditorGUI.LabelField(rect, "Global Offsets & Scale", EditorStyles.boldLabel);
                
                
                rect.x += 70;
                bool old_ChangedState = GUI.changed;

                GUI.changed = false;
                m_xOffset = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 0, rect.y + 20, width - 5f, 18), new GUIContent("OX:"), m_xOffset);
                if (GUI.changed) UpdateGlobalProperty("xOffset", m_xOffset);
                
                m_yOffset = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 1, rect.y + 20, width - 5f, 18), new GUIContent("OY:"), m_yOffset);
                if (GUI.changed) UpdateGlobalProperty("yOffset", m_yOffset);
                
                m_xAdvance = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 2, rect.y + 20, width - 5f, 18), new GUIContent("ADV."), m_xAdvance);
                if (GUI.changed) UpdateGlobalProperty("xAdvance", m_xAdvance);
                
                m_scale = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 3, rect.y + 20, width - 5f, 18), new GUIContent("SF."), m_scale);
                if (GUI.changed) UpdateGlobalProperty("scale", m_scale);

                EditorGUILayout.EndVertical();
                #endregion

                GUI.changed = old_ChangedState;
                
            }


            if (serializedObject.ApplyModifiedProperties() || evt_cmd == k_UndoRedo || isAssetDirty)
            {
                isAssetDirty = false;
                EditorUtility.SetDirty(target);
                //TMPro_EditorUtility.RepaintAll(); // Consider SetDirty
            }

            // Clear selection if mouse event was not consumed. 
            GUI.enabled = true;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                m_selectedElement = -1;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arraySize"></param>
        /// <param name="itemsPerPage"></param>
        void DisplayGlyphPageNavigation(int arraySize, int itemsPerPage)
        {
            Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
            pagePos.width /= 3;

            int shiftMultiplier = Event.current.shift ? 10 : 1; // Page + Shift goes 10 page forward

            // Previous Page
            GUI.enabled = m_page > 0;

            if (GUI.Button(pagePos, "Previous Page"))
            {
                m_page -= 1 * shiftMultiplier;
                //m_isNewPage = true;
            }

            // Page Counter
            var pageStyle = new GUIStyle(GUI.skin.button) { normal = { background = null } };
            GUI.enabled = true;
            pagePos.x += pagePos.width;
            int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);
            GUI.Button(pagePos, "Page " + (m_page + 1) + " / " + totalPages, pageStyle);

            // Next Page
            pagePos.x += pagePos.width;
            GUI.enabled = itemsPerPage * (m_page + 1) < arraySize;

            if (GUI.Button(pagePos, "Next Page"))
            {
                m_page += 1 * shiftMultiplier;
                //m_isNewPage = true;
            }

            // Clamp page range
            m_page = Mathf.Clamp(m_page, 0, arraySize / itemsPerPage);

            GUI.enabled = true;
        }


        /// <summary>
        /// Method to update the properties of all sprites
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void UpdateGlobalProperty(string property, float value)
        {
            int arraySize = m_spriteInfoList_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty spriteInfo = m_spriteInfoList_prop.GetArrayElementAtIndex(i);
                spriteInfo.FindPropertyRelative(property).floatValue = value;
            }

            GUI.changed = false;
        }

        // Check if any of the Style elements were clicked on.
        private bool DoSelectionCheck(Rect selectionArea)
        {
            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (selectionArea.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
                    {
                        currentEvent.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CopySerializedProperty(SerializedProperty source, ref SerializedProperty target)
        {
            target.FindPropertyRelative("name").stringValue = source.FindPropertyRelative("name").stringValue;
            target.FindPropertyRelative("hashCode").intValue = source.FindPropertyRelative("hashCode").intValue;
            target.FindPropertyRelative("x").floatValue = source.FindPropertyRelative("x").floatValue;
            target.FindPropertyRelative("y").floatValue = source.FindPropertyRelative("y").floatValue;
            target.FindPropertyRelative("width").floatValue = source.FindPropertyRelative("width").floatValue;
            target.FindPropertyRelative("height").floatValue = source.FindPropertyRelative("height").floatValue;
            target.FindPropertyRelative("xOffset").floatValue = source.FindPropertyRelative("xOffset").floatValue;
            target.FindPropertyRelative("yOffset").floatValue = source.FindPropertyRelative("yOffset").floatValue;
            target.FindPropertyRelative("xAdvance").floatValue = source.FindPropertyRelative("xAdvance").floatValue;
            target.FindPropertyRelative("scale").floatValue = source.FindPropertyRelative("scale").floatValue;
            target.FindPropertyRelative("sprite").objectReferenceValue = source.FindPropertyRelative("sprite").objectReferenceValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        void SearchGlyphTable(string searchPattern, ref List<int> searchResults)
        {
            if (searchResults == null) searchResults = new List<int>();
            searchResults.Clear();

            int arraySize = m_spriteInfoList_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty sourceSprite = m_spriteInfoList_prop.GetArrayElementAtIndex(i);

                // Check for potential match against decimal id
                int id = sourceSprite.FindPropertyRelative("id").intValue;
                if (id.ToString().Contains(searchPattern))
                    searchResults.Add(i);

                // Check for potential match against name
                string name = sourceSprite.FindPropertyRelative("name").stringValue.ToLower(System.Globalization.CultureInfo.InvariantCulture).Trim();
                if (name.Contains(searchPattern))
                    searchResults.Add(i);
            }
        }

    }
}