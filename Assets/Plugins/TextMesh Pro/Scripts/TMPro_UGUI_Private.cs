// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Release 1.0.54 

//#define PROFILE_ON
//#define PROFILE_PHASES_ON


using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

#pragma warning disable 0414 // Disabled a few warnings related to serialized variables not used in this script but used in the editor.
#pragma warning disable 0618 // Disabled warning due to SetVertices being deprecated until new release with SetMesh() is available.

namespace TMPro
{  
    public partial class TextMeshProUGUI
    {
        // UV Mapping Related Properties
        [SerializeField]
        private Vector2 m_uvOffset = Vector2.zero; // Used to offset UV on Texturing

        [SerializeField]
        private float m_uvLineOffset = 0.0f; // Used for UV line offset per line

        [SerializeField]
        private bool m_hasFontAssetChanged = false; // Used to track when font properties have changed.

        [SerializeField]
        protected TMP_SubMeshUI[] m_subTextObjects = new TMP_SubMeshUI[16];

        private float m_previousLossyScaleY = -1; // Used for Tracking lossy scale changes in the transform;

        private Vector3[] m_RectTransformCorners = new Vector3[4];
        private CanvasRenderer m_canvasRenderer;
        private Canvas m_canvas;


        private bool m_isFirstAllocation; // Flag to determine if this is the first allocation of the buffers.
        private int m_max_characters = 8; // Determines the initial allocation and size of the character array / buffer.
        //private int m_max_numberOfLines = 4; // Determines the initial allocation and maximum number of lines of text. 

        // Global Variables used in conjunction with saving the state of words or lines.
        private WordWrapState m_SavedWordWrapState = new WordWrapState(); // Struct which holds various states / variables used in conjunction with word wrapping.
        private WordWrapState m_SavedLineState = new WordWrapState();


        // MASKING RELATED PROPERTIES
        private bool m_isMaskingEnabled;
        // This property is now obsolete and used for compatibility with previous releases (prior to release 0.1.54).
        [SerializeField]
        private Material m_baseMaterial;


        private bool m_isScrollRegionSet;
        //private Mask m_mask;
        private int m_stencilID = 0;
      
        [SerializeField]
        private Vector4 m_maskOffset;

        // Matrix used to animated Env Map
        private Matrix4x4 m_EnvMapMatrix = new Matrix4x4();


        //private bool m_isEnabled;
        [NonSerialized]
        private bool m_isRegisteredForEvents;

        // DEBUG Variables
        //private System.Diagnostics.Stopwatch m_StopWatch;
        //private int frame = 0;
        private int m_recursiveCount = 0;
        private int m_recursiveCountA = 0;
        private int loopCountA = 0;
        //private int loopCountB = 0;
        //private int loopCountC = 0;
        //private int loopCountD = 0;
        //private int loopCountE = 0;

        //[SerializeField]
        //private new Material m_MaskMaterial;


        protected override void Awake()
        {
            //Debug.Log("***** Awake() *****");

            // Cache Reference to the Canvas
            m_canvas = this.canvas;
            m_isOrthographic = true;

            // Cache Reference to RectTransform.
            m_rectTransform = gameObject.GetComponent<RectTransform>();
            if (m_rectTransform == null)  
                m_rectTransform = gameObject.AddComponent<RectTransform>();

            // Cache a reference to the CanvasRenderer.
            m_canvasRenderer = GetComponent<CanvasRenderer>();
            if (m_canvasRenderer == null) 
                m_canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();

            if (m_mesh == null)
            {
                //Debug.Log("Creating new mesh.");
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.HideAndDontSave;

                //m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
            }

            // Load TMP Settings for new text object instances.
            if (m_text == null)
            {
                m_enableWordWrapping = TMP_Settings.enableWordWrapping;
                m_enableKerning = TMP_Settings.enableKerning;
                m_enableExtraPadding = TMP_Settings.enableExtraPadding;
                m_tintAllSprites = TMP_Settings.enableTintAllSprites;
                m_parseCtrlCharacters = TMP_Settings.enableParseEscapeCharacters;
            }

            // Load the font asset and assign material to renderer.
            LoadFontAsset();

            // Load Default TMP StyleSheet
            TMP_StyleSheet.LoadDefaultStyleSheet();

            // Allocate our initial buffers.
            m_char_buffer = new int[m_max_characters];
            m_cached_TextElement = new TMP_Glyph();
            m_isFirstAllocation = true;

            if (m_textInfo == null)
                m_textInfo = new TMP_TextInfo(this);

            // Check if we have a font asset assigned. Return if we don't because no one likes to see purple squares on screen.
            if (m_fontAsset == null)
            {
                Debug.LogWarning("Please assign a Font Asset to this " + transform.name + " gameobject.", this);
                return;
            }

            // Set Defaults for Font Auto-sizing
            if (m_fontSizeMin == 0) m_fontSizeMin = m_fontSize / 2;
            if (m_fontSizeMax == 0) m_fontSizeMax = m_fontSize * 2;

            // Set flags to ensure our text is parsed and redrawn.
            m_isInputParsingRequired = true;
            m_havePropertiesChanged = true;
            m_isCalculateSizeRequired = true;

            m_isAwake = true;
        }


        protected override void OnEnable()
        {
            //Debug.Log("*** OnEnable() ***", this);

            if (!m_isRegisteredForEvents)
            {
                //Debug.Log("Registering for Events.");
#if UNITY_EDITOR
                // Register Callbacks for various events.
                TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
                TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
                TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Add(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
                TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
                TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(ON_TEXT_STYLE_CHANGED);
                TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Add(ON_COLOR_GRADIENT_CHANGED);
                TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Add(ON_TMP_SETTINGS_CHANGED);
#endif
                m_isRegisteredForEvents = true;
            }

            // Cache Reference to the Canvas
            m_canvas = GetCanvas();

            SetActiveSubMeshes(true);

            // Register Graphic Component to receive event triggers
            GraphicRegistry.RegisterGraphicForCanvas(m_canvas, this);

            ComputeMarginSize();

            m_verticesAlreadyDirty = false;
            m_layoutAlreadyDirty = false;
            m_ShouldRecalculateStencil = true;
            m_isInputParsingRequired = true;
            SetAllDirty();

            RecalculateClipping();
        }


        protected override void OnDisable()
        {
            //base.OnDisable();
            //Debug.Log("***** OnDisable() *****"); //for " + this.name + " with ID: " + this.GetInstanceID() + " has been called.");

            if (m_MaskMaterial != null)
            { 
                TMP_MaterialManager.ReleaseStencilMaterial(m_MaskMaterial);
                m_MaskMaterial = null;
            }

            // UnRegister Graphic Component
            GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, this);
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild((ICanvasElement)this);

            if (m_canvasRenderer != null)
                m_canvasRenderer.Clear();

            SetActiveSubMeshes(false);

            LayoutRebuilder.MarkLayoutForRebuild(m_rectTransform);
            RecalculateClipping();
        }


        protected override void OnDestroy()
        {
            //base.OnDestroy();
            //Debug.Log("***** OnDestroy() *****");

            // UnRegister Graphic Component
            GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, this);

            // Clean up remaining mesh
            if (m_mesh != null)
                DestroyImmediate(m_mesh);

            // Clean up mask material
            if (m_MaskMaterial != null)
            {
                TMP_MaterialManager.ReleaseStencilMaterial(m_MaskMaterial);
                m_MaskMaterial = null;
            }

#if UNITY_EDITOR
            // Unregister the event this object was listening to
            TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
            TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
            TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Remove(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
            TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
            TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Remove(ON_TEXT_STYLE_CHANGED);
            TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Remove(ON_COLOR_GRADIENT_CHANGED);
            TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Remove(ON_TMP_SETTINGS_CHANGED);
#endif
            m_isRegisteredForEvents = false;
        }


#if UNITY_EDITOR
        protected override void Reset()
        {
            //base.Reset();
            //Debug.Log("***** Reset() *****"); //has been called.");
            m_isInputParsingRequired = true;
            m_havePropertiesChanged = true;
        }


        protected override void OnValidate()
        {
            //Debug.Log("***** OnValidate() ***** Frame:" + Time.frameCount); // ID " + GetInstanceID()); // New Material [" + m_sharedMaterial.name + "] with ID " + m_sharedMaterial.GetInstanceID() + ". Base Material is [" + m_baseMaterial.name + "] with ID " + m_baseMaterial.GetInstanceID() + ". Previous Base Material is [" + (m_lastBaseMaterial == null ? "Null" : m_lastBaseMaterial.name) + "].");

            // Handle Font Asset changes in the inspector.
            if (m_fontAsset == null || m_hasFontAssetChanged)
            {
                LoadFontAsset();
                m_isCalculateSizeRequired = true;
                m_hasFontAssetChanged = false;
            }


            if (m_canvasRenderer == null || m_canvasRenderer.GetMaterial() == null || m_canvasRenderer.GetMaterial().mainTexture == null || m_fontAsset == null || m_fontAsset.atlas.GetInstanceID() != m_canvasRenderer.GetMaterial().mainTexture.GetInstanceID())
            {
                LoadFontAsset();
                m_isCalculateSizeRequired = true;
                m_hasFontAssetChanged = false;
            }

            m_padding = GetPaddingForMaterial();
            //ComputeMarginSize();

            m_isInputParsingRequired = true;
            m_inputSource = TextInputSources.Text;
            m_havePropertiesChanged = true;
            m_isCalculateSizeRequired = true;
            m_isPreferredWidthDirty = true;
            m_isPreferredHeightDirty = true;

            SetAllDirty();
        }



        // Event received when custom material editor properties are changed.
        void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
        {
            //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED event received."); // Targeted Material is: " + mat.name + "  m_sharedMaterial: " + m_sharedMaterial.name + " with ID:" + m_sharedMaterial.GetInstanceID() + "  m_renderer.sharedMaterial: " + m_canvasRenderer.GetMaterial() + "  Masking Material:" + m_MaskMaterial.GetInstanceID());

            ShaderUtilities.GetShaderPropertyIDs(); // Initialize ShaderUtilities and get shader property IDs.

            int materialID = mat.GetInstanceID();
            int sharedMaterialID = m_sharedMaterial.GetInstanceID();
            int maskingMaterialID = m_MaskMaterial == null ? 0 : m_MaskMaterial.GetInstanceID();

            if (m_canvasRenderer == null || m_canvasRenderer.GetMaterial() == null)
            {
                if (m_canvasRenderer == null) return;

                if (m_fontAsset != null)
                {
                    m_canvasRenderer.SetMaterial(m_fontAsset.material, m_sharedMaterial.mainTexture);
                    //Debug.LogWarning("No Material was assigned to " + name + ". " + m_fontAsset.material.name + " was assigned.");
                }
                else
                    Debug.LogWarning("No Font Asset assigned to " + name + ". Please assign a Font Asset.", this);
            }


            if (m_canvasRenderer.GetMaterial() != m_sharedMaterial && m_fontAsset == null) //    || m_renderer.sharedMaterials.Contains(mat))
            {
                //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED Called on Target ID: " + GetInstanceID() + ". Previous Material:" + m_sharedMaterial + "  New Material:" + m_uiRenderer.GetMaterial()); // on Object ID:" + GetInstanceID() + ". m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial.name);         
                m_sharedMaterial = m_canvasRenderer.GetMaterial();
            }


            // Make sure material properties are synchronized between the assigned material and masking material.
            if (m_MaskMaterial != null)
            {
                UnityEditor.Undo.RecordObject(m_MaskMaterial, "Material Property Changes");
                UnityEditor.Undo.RecordObject(m_sharedMaterial, "Material Property Changes");

                if (materialID == sharedMaterialID)
                {
                    //Debug.Log("Copy base material properties to masking material if not null.");
                    float stencilID = m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilID);
                    float stencilComp = m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilComp);
                    //float stencilOp = m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilOp);
                    //float stencilRead = m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilReadMask);
                    //float stencilWrite = m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilWriteMask);

                    m_MaskMaterial.CopyPropertiesFromMaterial(mat);
                    m_MaskMaterial.shaderKeywords = mat.shaderKeywords;

                    m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
                    m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilComp, stencilComp);
                    //m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilOp, stencilOp);
                    //m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilReadMask, stencilID);
                    //m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilWriteMask, 0);
                }
                else if (materialID == maskingMaterialID)
                {
                    // Update the padding 
                    GetPaddingForMaterial(mat);

                    m_sharedMaterial.CopyPropertiesFromMaterial(mat);
                    m_sharedMaterial.shaderKeywords = mat.shaderKeywords;
                    m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilID, 0);
                    m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilComp, 8);
                    //m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilOp, 0);
                    //m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilReadMask, 255);
                    //m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilWriteMask, 255);
                }

            }

            m_padding = GetPaddingForMaterial();
            m_havePropertiesChanged = true;
            SetVerticesDirty();
            //SetMaterialDirty();
        }


        // Event received when font asset properties are changed in Font Inspector
        void ON_FONT_PROPERTY_CHANGED(bool isChanged, TMP_FontAsset font)
        {
            if (MaterialReference.Contains(m_materialReferences, font))
            {
                //Debug.Log("ON_FONT_PROPERTY_CHANGED event received.");
                m_isInputParsingRequired = true;
                m_havePropertiesChanged = true;

                SetLayoutDirty();
                SetVerticesDirty();
            }
        }


        // Event received when UNDO / REDO Event alters the properties of the object.
        void ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(bool isChanged, TextMeshProUGUI obj)
        {
            Debug.Log("Event Received by " + obj);
            
            if (obj == this)
            {
                //Debug.Log("Undo / Redo Event Received by Object ID:" + GetInstanceID());
                m_havePropertiesChanged = true;
                m_isInputParsingRequired = true;

                SetVerticesDirty();
            }
        }


        // Event to Track Material Changed resulting from Drag-n-drop.
        void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
        {
            //Debug.Log("Drag-n-Drop Event - Receiving Object ID " + GetInstanceID() + ". Sender ID " + obj.GetInstanceID()); // +  ". Prefab Parent is " + UnityEditor.PrefabUtility.GetPrefabParent(gameObject).GetInstanceID()); // + ". New Material is " + newMaterial.name + " with ID " + newMaterial.GetInstanceID() + ". Base Material is " + m_baseMaterial.name + " with ID " + m_baseMaterial.GetInstanceID());

            // Check if event applies to this current object
            if (obj == gameObject || UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == obj)
            {
                UnityEditor.Undo.RecordObject(this, "Material Assignment");
                UnityEditor.Undo.RecordObject(m_canvasRenderer, "Material Assignment");

                m_sharedMaterial = newMaterial;

                m_padding = GetPaddingForMaterial();

                m_havePropertiesChanged = true;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }


        // Event received when Text Styles are changed.
        void ON_TEXT_STYLE_CHANGED(bool isChanged)
        {
            m_havePropertiesChanged = true;
            m_isInputParsingRequired = true;
            SetVerticesDirty();
        }


        /// <summary>
        /// Event received when a Color Gradient Preset is modified.
        /// </summary>
        /// <param name="textObject"></param>
        void ON_COLOR_GRADIENT_CHANGED(TMP_ColorGradient gradient)
        {
            if (m_fontColorGradientPreset != null && gradient.GetInstanceID() == m_fontColorGradientPreset.GetInstanceID())
            {
                m_havePropertiesChanged = true;
                SetVerticesDirty();
            }
        }


        /// <summary>
        /// Event received when the TMP Settings are changed.
        /// </summary>
        void ON_TMP_SETTINGS_CHANGED()
        {
            m_defaultSpriteAsset = null;
            m_havePropertiesChanged = true;
            m_isInputParsingRequired = true;
            SetAllDirty();
        }
#endif


        // Function which loads either the default font or a newly assigned font asset. This function also assigned the appropriate material to the renderer.
        protected override void LoadFontAsset()
        {
            //Debug.Log("***** LoadFontAsset() *****"); //TextMeshPro LoadFontAsset() has been called."); // Current Font Asset is " + (font != null ? font.name: "Null") );
            
            ShaderUtilities.GetShaderPropertyIDs(); // Initialize & Get shader property IDs.

            if (m_fontAsset == null)
            {
                if (TMP_Settings.defaultFontAsset != null)
                    m_fontAsset = TMP_Settings.defaultFontAsset;
                else
                    m_fontAsset = Resources.Load("Fonts & Materials/ARIAL SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;

                if (m_fontAsset == null)
                {
                    Debug.LogWarning("The ARIAL SDF Font Asset was not found. There is no Font Asset assigned to " + gameObject.name + ".", this);
                    return;
                }

                if (m_fontAsset.characterDictionary == null)
                {
                    Debug.Log("Dictionary is Null!");
                }

                m_sharedMaterial = m_fontAsset.material;
            }
            else
            {
                // Read font definition if needed.
                if (m_fontAsset.characterDictionary == null)
                    m_fontAsset.ReadFontDefinition();

                // Added for compatibility with previous releases.
                if (m_sharedMaterial == null && m_baseMaterial != null)
                {
                    m_sharedMaterial = m_baseMaterial;
                    m_baseMaterial = null;
                }

                // If font atlas texture doesn't match the assigned material font atlas, switch back to default material specified in the Font Asset.
                if (m_sharedMaterial == null || m_sharedMaterial.mainTexture == null || m_fontAsset.atlas.GetInstanceID() != m_sharedMaterial.mainTexture.GetInstanceID())
                {
                    if (m_fontAsset.material == null)
                        Debug.LogWarning("The Font Atlas Texture of the Font Asset " + m_fontAsset.name + " assigned to " + gameObject.name + " is missing.", this);
                    else
                        m_sharedMaterial = m_fontAsset.material;
                }
            }


            // Find and cache Underline & Ellipsis characters.
            GetSpecialCharacters(m_fontAsset);

            m_padding = GetPaddingForMaterial();

            SetMaterialDirty();
        }


        /// <summary>
        /// Method to retrieve the parent Canvas.
        /// </summary>
        private Canvas GetCanvas()
        {
            Canvas canvas = null;
            var list = TMP_ListPool<Canvas>.Get();

            gameObject.GetComponentsInParent(false, list);
            if (list.Count > 0)
            {
                // Find the first active and enabled canvas.
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].isActiveAndEnabled)
                    {
                        canvas = list[i];
                        break;
                    }
                }
            }

            TMP_ListPool<Canvas>.Release(list);

            return canvas;
        }


        /// <summary>
        /// Method used when animating the Env Map on the material.
        /// </summary>
        void UpdateEnvMapMatrix()
        {
            if (!m_sharedMaterial.HasProperty(ShaderUtilities.ID_EnvMap) || m_sharedMaterial.GetTexture(ShaderUtilities.ID_EnvMap) == null)
                return;

            //Debug.Log("Updating Env Matrix...");
            Vector3 rotation = m_sharedMaterial.GetVector(ShaderUtilities.ID_EnvMatrixRotation);
            m_EnvMapMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rotation), Vector3.one);

            m_sharedMaterial.SetMatrix(ShaderUtilities.ID_EnvMatrix, m_EnvMapMatrix);
        }


        // Enable Masking in the Shader
        void EnableMasking()
        {
            if (m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_canvasRenderer.SetMaterial(m_fontMaterial, m_sharedMaterial.mainTexture);
            }

            m_sharedMaterial = m_fontMaterial;
            if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
            {
                m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);

                UpdateMask(); // Update Masking Coordinates
            }

            m_isMaskingEnabled = true;

            //m_uiRenderer.SetMaterial(m_sharedMaterial, null);

            //m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);

            /*
            Material mat = m_uiRenderer.GetMaterial();
            if (mat.HasProperty(ShaderUtilities.ID_MaskCoord))
            {
                mat.EnableKeyword("MASK_SOFT");
                mat.DisableKeyword("MASK_HARD");
                mat.DisableKeyword("MASK_OFF");

                m_isMaskingEnabled = true;
                UpdateMask();
            }
            */
        }


        // Enable Masking in the Shader
        void DisableMasking()
        {
            if (m_fontMaterial != null)
            {
                if (m_stencilID > 0)
                    m_sharedMaterial = m_MaskMaterial;
                //else
                //    m_sharedMaterial = m_baseMaterial;
                           
                m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);

                DestroyImmediate(m_fontMaterial);
            }
              
            m_isMaskingEnabled = false;
            
            /*
            if (m_maskingMaterial != null && m_stencilID == 0)
            {
                m_sharedMaterial = m_baseMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, null);
            }
            else if (m_stencilID > 0)
            {
                m_sharedMaterial.EnableKeyword("MASK_OFF");
                m_sharedMaterial.DisableKeyword("MASK_HARD");
                m_sharedMaterial.DisableKeyword("MASK_SOFT");
            }
            */
             
          
            /*
            Material mat = m_uiRenderer.GetMaterial();
            if (mat.HasProperty(ShaderUtilities.ID_MaskCoord))
            {
                mat.EnableKeyword("MASK_OFF");
                mat.DisableKeyword("MASK_HARD");
                mat.DisableKeyword("MASK_SOFT");

                m_isMaskingEnabled = false;
                UpdateMask();
            }
            */
        }


        // Update & recompute Mask offset
        void UpdateMask()
        {
            //Debug.Log("Updating Mask...");

            if (m_rectTransform != null)
            {
                //Material mat = m_uiRenderer.GetMaterial();
                //if (mat == null || (m_overflowMode == TextOverflowModes.ScrollRect && m_isScrollRegionSet))
                //    return;

                if (!ShaderUtilities.isInitialized)
                    ShaderUtilities.GetShaderPropertyIDs();
                
                //Debug.Log("Setting Mask for the first time.");

                m_isScrollRegionSet = true;

                float softnessX = Mathf.Min(Mathf.Min(m_margin.x, m_margin.z), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX));
                float softnessY = Mathf.Min(Mathf.Min(m_margin.y, m_margin.w), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessY));

                softnessX = softnessX > 0 ? softnessX : 0;
                softnessY = softnessY > 0 ? softnessY : 0;

                float width = (m_rectTransform.rect.width - Mathf.Max(m_margin.x, 0) - Mathf.Max(m_margin.z, 0)) / 2 + softnessX;
                float height = (m_rectTransform.rect.height - Mathf.Max(m_margin.y, 0) - Mathf.Max(m_margin.w, 0)) / 2 + softnessY;

                
                Vector2 center = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (Mathf.Max(m_margin.x, 0) - Mathf.Max(m_margin.z, 0)) / 2, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (-Mathf.Max(m_margin.y, 0) + Mathf.Max(m_margin.w, 0)) / 2);
        
                //Vector2 center = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (margin.x - margin.z) / 2, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (-margin.y + margin.w) / 2);
                Vector4 mask = new Vector4(center.x, center.y, width, height);
                //Debug.Log(mask);



                //Rect rect = new Rect(0, 0, m_rectTransform.rect.width + margin.x + margin.z, m_rectTransform.rect.height + margin.y + margin.w);
                //int softness = (int)m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX) / 2;
                m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, mask);
            }
        }


        // Function called internally when a new material is assigned via the fontMaterial property.
        protected override Material GetMaterial(Material mat)
        {
            // Get Shader PropertyIDs if they haven't been cached already.
            ShaderUtilities.GetShaderPropertyIDs();
            
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            //if (m_canvasRenderer == null)
            //    m_canvasRenderer = GetComponent<CanvasRenderer>();

            // Create Instance Material only if the new material is not the same instance previously used.
            if (m_fontMaterial == null || m_fontMaterial.GetInstanceID() != mat.GetInstanceID())
                m_fontMaterial = CreateMaterialInstance(mat);

            m_sharedMaterial = m_fontMaterial;

            m_padding = GetPaddingForMaterial();

            m_ShouldRecalculateStencil = true;
            SetVerticesDirty();
            SetMaterialDirty();

            return m_sharedMaterial;
        }


        /// <summary>
        /// Method returning instances of the materials used by the text object.
        /// </summary>
        /// <returns></returns>
        protected override Material[] GetMaterials(Material[] mats)
        {
            int materialCount = m_textInfo.materialCount;

            if (m_fontMaterials == null)
                m_fontMaterials = new Material[materialCount];
            else if (m_fontMaterials.Length != materialCount)
                TMP_TextInfo.Resize(ref m_fontMaterials, materialCount, false);

            // Get instances of the materials
            for (int i = 0; i < materialCount; i++)
            {
                if (i == 0)
                    m_fontMaterials[i] = fontMaterial;
                else
                    m_fontMaterials[i] = m_subTextObjects[i].material;
            }

            m_fontSharedMaterials = m_fontMaterials;

            return m_fontMaterials;
        }


        // Function called internally when a new shared material is assigned via the fontSharedMaterial property.
        protected override void SetSharedMaterial(Material mat) 
        {
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object. 
            //if (m_canvasRenderer == null)
            //    m_canvasRenderer = GetComponent<CanvasRenderer>();

            m_sharedMaterial = mat;

            m_padding = GetPaddingForMaterial();

            SetMaterialDirty();
        }


        /// <summary>
        /// Method returning an array containing the materials used by the text object.
        /// </summary>
        /// <returns></returns>
        protected override Material[] GetSharedMaterials()
        {
            int materialCount = m_textInfo.materialCount;

            if (m_fontSharedMaterials == null)
                m_fontSharedMaterials = new Material[materialCount];
            else if (m_fontSharedMaterials.Length != materialCount)
                TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, false);

            for (int i = 0; i < materialCount; i++)
            {
                if (i == 0)
                    m_fontSharedMaterials[i] = m_sharedMaterial;
                else
                    m_fontSharedMaterials[i] = m_subTextObjects[i].sharedMaterial;
            }

            return m_fontSharedMaterials;
        }


        /// <summary>
        /// Method used to assign new materials to the text and sub text objects.
        /// </summary>
        protected override void SetSharedMaterials(Material[] materials)
        {
            int materialCount = m_textInfo.materialCount;

            // Check allocation of the fontSharedMaterials array.
            if (m_fontSharedMaterials == null)
                m_fontSharedMaterials = new Material[materialCount];
            else if (m_fontSharedMaterials.Length != materialCount)
                TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, false);

            // Only assign as many materials as the text object contains.
            for (int i = 0; i < materialCount; i++)
            {
                if (i == 0)
                {
                    // Only assign new material if the font atlas textures match.
                    if (materials[i].mainTexture == null || materials[i].mainTexture.GetInstanceID() != m_sharedMaterial.mainTexture.GetInstanceID())
                        continue;

                    m_sharedMaterial = m_fontSharedMaterials[i] = materials[i];
                    m_padding = GetPaddingForMaterial(m_sharedMaterial);
                }
                else
                {
                    // Only assign new material if the font atlas textures match.
                    if (materials[i].mainTexture == null || materials[i].mainTexture.GetInstanceID() != m_subTextObjects[i].sharedMaterial.mainTexture.GetInstanceID())
                        continue;

                    // Only assign a new material if none were specified in the text input.
                    if (m_subTextObjects[i].isDefaultMaterial)
                        m_subTextObjects[i].sharedMaterial = m_fontSharedMaterials[i] = materials[i];
                }
            }
        }


        // This function will create an instance of the Font Material.
        protected override void SetOutlineThickness(float thickness)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
            {
                m_sharedMaterial = m_fontMaterial;
                m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }
            else if(m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_sharedMaterial = m_fontMaterial;
                m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }

            thickness = Mathf.Clamp01(thickness);
            m_sharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
            m_padding = GetPaddingForMaterial();
        }


        // This function will create an instance of the Font Material.
        protected override void SetFaceColor(Color32 color)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial == null)
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);

            m_sharedMaterial = m_fontMaterial;
            m_padding = GetPaddingForMaterial();

            m_sharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
        }


        // This function will create an instance of the Font Material.
        protected override void SetOutlineColor(Color32 color)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial == null)
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);

            m_sharedMaterial = m_fontMaterial;
            m_padding = GetPaddingForMaterial();

            m_sharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, color);
        }


        // Sets the Render Queue and Ztest mode 
        protected override void SetShaderDepth()
        {
            if (m_canvas == null || m_sharedMaterial == null)
                return;

            if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay || m_isOverlay)
            {
                // Should this use an instanced material?
                m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 0);
            }
            else
            {   // TODO: This section needs to be tested.
                m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4);
            }
        }


        // Sets the Culling mode of the material
        protected override void SetCulling()
        {
            if (m_isCullingEnabled)
            {
                m_canvasRenderer.GetMaterial().SetFloat("_CullMode", 2);
            }
            else
            {
                m_canvasRenderer.GetMaterial().SetFloat("_CullMode", 0);
            }
        }


        // Set Perspective Correction Mode based on whether Camera is Orthographic or Perspective
        void SetPerspectiveCorrection()
        {
            if (m_isOrthographic)
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.0f);
            else
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.875f);
        }


        /// <summary>
        /// Get the padding value for the currently assigned material.
        /// </summary>
        /// <returns></returns>
        protected override float GetPaddingForMaterial(Material mat)
        {
            m_padding = ShaderUtilities.GetPadding(mat, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = mat.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }


        /// <summary>
        /// Get the padding value for the currently assigned material.
        /// </summary>
        /// <returns></returns>
        protected override float GetPaddingForMaterial()
        {
            ShaderUtilities.GetShaderPropertyIDs();

            m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }

        // Function to allocate the necessary buffers to render the text. This function is called whenever the buffer size needs to be increased.
        void SetMeshArrays(int size)
        {
            m_textInfo.meshInfo[0].ResizeMeshInfo(size);

            m_canvasRenderer.SetMesh(m_textInfo.meshInfo[0].mesh);
        }


        // This function parses through the Char[] to determine how many characters will be visible. It then makes sure the arrays are large enough for all those characters.
        protected override int SetArraySizes(int[] chars)
        {
            //Debug.Log("*** SetArraySizes() ***");
            #if PROFILE_ON
            Profiler.BeginSample("SetArraySizes");
            #endif

            int tagEnd = 0;
            int spriteCount = 0;

            m_totalCharacterCount = 0;
            m_isUsingBold = false;
            m_isParsingText = false;
            tag_NoParsing = false;
            m_style = m_fontStyle;

            m_fontWeightInternal = (m_style & FontStyles.Bold) == FontStyles.Bold ? 700 : m_fontWeight;
            m_fontWeightStack.SetDefault(m_fontWeightInternal);

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;
            m_currentMaterialIndex = 0;

            m_materialReferenceStack.SetDefault(new MaterialReference(0, m_currentFontAsset, null, m_currentMaterial, m_padding));

            m_materialReferenceIndexLookup.Clear();
            MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);

            if (m_textInfo == null) m_textInfo = new TMP_TextInfo();
            m_textElementType = TMP_TextElementType.Character;

            // Parsing XML tags in the text
            for (int i = 0; chars[i] != 0; i++)
            {
                //Make sure the characterInfo array can hold the next text element.
                if (m_textInfo.characterInfo == null || m_totalCharacterCount >= m_textInfo.characterInfo.Length)
                    TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_totalCharacterCount + 1, true);

                int c = chars[i];

                // PARSE XML TAGS
                #region PARSE XML TAGS
                if (m_isRichText && c == 60) // if Char '<'
                {
                    int prev_MaterialIndex = m_currentMaterialIndex;

                    // Check if Tag is Valid
                    if (ValidateHtmlTag(chars, i + 1, out tagEnd))
                    {
                        i = tagEnd;
                        //if ((m_style & FontStyles.Underline) == FontStyles.Underline) visibleCount += 3;

                        if ((m_style & FontStyles.Bold) == FontStyles.Bold) m_isUsingBold = true;

                        if (m_textElementType == TMP_TextElementType.Sprite)
                        {
                            m_materialReferences[m_currentMaterialIndex].referenceCount += 1;

                            m_textInfo.characterInfo[m_totalCharacterCount].character = (char)(57344 + m_spriteIndex);
                            m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
                            m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;

                            // Restore element type and material index to previous values.
                            m_textElementType = TMP_TextElementType.Character;
                            m_currentMaterialIndex = prev_MaterialIndex;

                            spriteCount += 1;
                            m_totalCharacterCount += 1;
                        }

                        continue;
                    }
                }
                #endregion


                bool isUsingFallback = false;
                bool isUsingAlternativeTypeface = false;

                TMP_Glyph glyph;
                TMP_FontAsset tempFontAsset;
                TMP_FontAsset prev_fontAsset = m_currentFontAsset;
                Material prev_material = m_currentMaterial;
                int prev_materialIndex = m_currentMaterialIndex;


                // Handle Font Styles like LowerCase, UpperCase and SmallCaps.
                #region Handling of LowerCase, UpperCase and SmallCaps Font Styles
                if (m_textElementType == TMP_TextElementType.Character)
                {
                    if ((m_style & FontStyles.UpperCase) == FontStyles.UpperCase)
                    {
                        // If this character is lowercase, switch to uppercase.
                        if (char.IsLower((char)c))
                            c = char.ToUpper((char)c);

                    }
                    else if ((m_style & FontStyles.LowerCase) == FontStyles.LowerCase)
                    {
                        // If this character is uppercase, switch to lowercase.
                        if (char.IsUpper((char)c))
                            c = char.ToLower((char)c);
                    }
                    else if ((m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps)
                    {
                        // Only convert lowercase characters to uppercase.
                        if (char.IsLower((char)c))
                            c = char.ToUpper((char)c);
                    }
                }
                #endregion


                // Handling of font weights.
                #region HANDLING OF FONT WEIGHT
                tempFontAsset = GetFontAssetForWeight(m_fontWeightInternal);
                if (tempFontAsset != null)
                {
                    isUsingFallback = true;
                    isUsingAlternativeTypeface = true;
                    m_currentFontAsset = tempFontAsset;
                    //m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                }
                #endregion


                // Lookup the Glyph data for each character and cache it.
                #region LOOKUP GLYPH
                if (m_currentFontAsset.characterDictionary.TryGetValue(c, out glyph) == false)
                {

                    // Check current font asset font fallback list.
                    if (m_currentFontAsset.fallbackFontAssets != null && m_currentFontAsset.fallbackFontAssets.Count > 0)
                    {
                        // Iterate through each Fallback Font Asset looking for the missing character.
                        for (int y = 0; y < m_currentFontAsset.fallbackFontAssets.Count; y++)
                        {
                            tempFontAsset = m_currentFontAsset.fallbackFontAssets[y];

                            if (tempFontAsset == null) continue;

                            if (tempFontAsset.characterDictionary.TryGetValue(c, out glyph))
                            {
                                isUsingFallback = true;
                                m_currentFontAsset = tempFontAsset;
                                break;
                            }
                        }
                    }

                    // Check TMP Settings font asset fallback list.
                    if (glyph == null)
                    {
                        if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                        {
                            // Iterate through each Fallback Font Asset looking for the missing character.
                            for (int y = 0; y < TMP_Settings.fallbackFontAssets.Count; y++)
                            {
                                tempFontAsset = TMP_Settings.fallbackFontAssets[y];

                                if (tempFontAsset == null) continue;

                                if (tempFontAsset.characterDictionary.TryGetValue(c, out glyph))
                                {
                                    isUsingFallback = true;
                                    m_currentFontAsset = tempFontAsset;
                                    break;
                                }
                            }
                        }
                    }

                    // Check if Lowercase or Uppercase variant of the character is available.
                    if (glyph == null)
                    {
                        if (char.IsLower((char)c))
                        {
                            if (m_currentFontAsset.characterDictionary.TryGetValue(char.ToUpper((char)c), out glyph))
                                c = chars[i] = char.ToUpper((char)c);
                        }
                        else if (char.IsUpper((char)c))
                        {
                            if (m_currentFontAsset.characterDictionary.TryGetValue(char.ToLower((char)c), out glyph))
                                c = chars[i] = char.ToLower((char)c);
                        }
                    }

                    // Since the missing character is still unavailable, try replacing it by TMP Settings Missing Glyph or Square (9633) character.
                    if (glyph == null)
                    {
                        // Check for the missing glyph character in the currently assigned font asset.
                        int altGlyphCode = TMP_Settings.missingGlyphCharacter == 0 ? 9633 : TMP_Settings.missingGlyphCharacter;
                        if (m_currentFontAsset.characterDictionary.TryGetValue(altGlyphCode, out glyph))
                        {
                            if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Character with ASCII value of " + c + " was not found in the Font Asset Glyph Table.", this);

                            // Replace the missing character by square missing glyph character
                            c = chars[i] = altGlyphCode;
                        }
                        else
                        {
                            // Check for the missing glyph character in the Font Fallback list.
                            if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                            {
                                // Iterate through each Fallback Font Asset looking for the missing character.
                                for (int y = 0; y < TMP_Settings.fallbackFontAssets.Count; y++)
                                {
                                    tempFontAsset = TMP_Settings.fallbackFontAssets[y];

                                    if (tempFontAsset == null) continue;

                                    if (tempFontAsset.characterDictionary.TryGetValue(altGlyphCode, out glyph))
                                    {
                                        // Use Square Missing Glyph from Default Font Asset.
                                        if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Character with ASCII value of " + c + " was not found in the Font Asset Glyph Table.", this);

                                        c = chars[i] = altGlyphCode;

                                        isUsingFallback = true;
                                        m_currentFontAsset = tempFontAsset;
                                        //m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                                        break;
                                    }
                                }
                            }

                            // Check for the missing glyph character in the Default Font Asset assigned in the TMP Settings file.
                            if (glyph == null)
                            {
                                tempFontAsset = TMP_Settings.GetFontAsset();
                                if (tempFontAsset != null && tempFontAsset.characterDictionary.TryGetValue(altGlyphCode, out glyph))
                                {
                                    // Use Square Missing Glyph from Default Font Asset.
                                    if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Character with ASCII value of " + c + " was not found in the Font Asset Glyph Table.", this);

                                    c = chars[i] = altGlyphCode;

                                    isUsingFallback = true;
                                    m_currentFontAsset = tempFontAsset;
                                    //m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                                }
                                else
                                {
                                    // Get a reference to ARIAL SDF
                                    tempFontAsset = TMP_FontAsset.defaultFontAsset;
                                    if (tempFontAsset != null && tempFontAsset.characterDictionary.TryGetValue(altGlyphCode, out glyph))
                                    {
                                        // Use Square Missing Glyph from ARIAL SDF
                                        if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Character with ASCII value of " + c + " was not found in the Font Asset Glyph Table.", this);

                                        c = chars[i] = altGlyphCode;

                                        isUsingFallback = true;
                                        m_currentFontAsset = tempFontAsset;
                                        //m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                                    }
                                    else
                                    {
                                        // Replacing missing glyph by Space (32) since no suitable replacement has been found.
                                        if (m_currentFontAsset.characterDictionary.TryGetValue(32, out glyph))
                                        {
                                            // Use Space (32) Glyph
                                            if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Character with ASCII value of " + c + " was not found in the Font Asset Glyph Table. It was replaced by a space.", this);
                                            c = chars[i] = 32;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion


                m_textInfo.characterInfo[m_totalCharacterCount].textElement = glyph;
                m_textInfo.characterInfo[m_totalCharacterCount].isUsingAlternateTypeface = isUsingAlternativeTypeface;
                m_textInfo.characterInfo[m_totalCharacterCount].character = (char)c;
                m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;

                if (isUsingFallback)
                {
                    // Create Fallback material instance matching current material preset if necessary
                    if (TMP_Settings.matchMaterialPreset)
                        m_currentMaterial = TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial, m_currentFontAsset.material);
                    else
                        m_currentMaterial = m_currentFontAsset.material;

                    m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                }

                if (!char.IsWhiteSpace((char)c))
                {
                    // Limit the mesh of the main text object to 65535 vertices and use sub objects for the overflow.
                    if (m_materialReferences[m_currentMaterialIndex].referenceCount < 16383)
                        m_materialReferences[m_currentMaterialIndex].referenceCount += 1;
                    else
                    {
                        m_currentMaterialIndex = MaterialReference.AddMaterialReference(new Material(m_currentMaterial), m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
                        m_materialReferences[m_currentMaterialIndex].referenceCount += 1;
                    }
                }

                m_textInfo.characterInfo[m_totalCharacterCount].material = m_currentMaterial;
                m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
                m_materialReferences[m_currentMaterialIndex].isFallbackMaterial = isUsingFallback;

                // Restore previous font asset and material if fallback font was used.
                if (isUsingFallback)
                {
                    m_materialReferences[m_currentMaterialIndex].fallbackMaterial = prev_material;
                    m_currentFontAsset = prev_fontAsset;
                    m_currentMaterial = prev_material;
                    m_currentMaterialIndex = prev_materialIndex;
                }

                m_totalCharacterCount += 1;
            }

            // Early return if we are calculating the preferred values.
            if (m_isCalculatingPreferredValues)
            {
                m_isCalculatingPreferredValues = false;
                m_isInputParsingRequired = true;
                return m_totalCharacterCount;
            }

            // Save material and sprite count.
            m_textInfo.spriteCount = spriteCount;
            int materialCount = m_textInfo.materialCount = m_materialReferenceIndexLookup.Count;

            // Check if we need to resize the MeshInfo array for handling different materials.
            if (materialCount > m_textInfo.meshInfo.Length)
                TMP_TextInfo.Resize(ref m_textInfo.meshInfo, materialCount, false);

            // Resize CharacterInfo[] if allocations are excessive
            if (m_textInfo.characterInfo.Length - m_totalCharacterCount > 256)
                TMP_TextInfo.Resize(ref m_textInfo.characterInfo, Mathf.Max(m_totalCharacterCount + 1, 256), true);


            // Iterate through the material references to set the mesh buffer allocations
            for (int i = 0; i < materialCount; i++)
            {
                // Add new sub text object for each material reference
                if (i > 0)
                {
                    if (m_subTextObjects[i] == null)
                    {
                        m_subTextObjects[i] = TMP_SubMeshUI.AddSubTextObject(this, m_materialReferences[i]);

                        // Not sure this is necessary
                        m_textInfo.meshInfo[i].vertices = null;
                    }
                    //else if (m_subTextObjects[i].gameObject.activeInHierarchy == false)
                    //    m_subTextObjects[i].gameObject.SetActive(true);

                    // Make sure the pivots are synchronized
                    if (m_rectTransform.pivot != m_subTextObjects[i].rectTransform.pivot)
                        m_subTextObjects[i].rectTransform.pivot = m_rectTransform.pivot;

                    // Check if the material has changed.
                    if (m_subTextObjects[i].sharedMaterial == null || m_subTextObjects[i].sharedMaterial.GetInstanceID() != m_materialReferences[i].material.GetInstanceID())
                    {
                        bool isDefaultMaterial = m_materialReferences[i].isDefaultMaterial;

                        m_subTextObjects[i].isDefaultMaterial = isDefaultMaterial;

                        // Assign new material if we are not using the default material or if the font asset has changed.
                        if (!isDefaultMaterial || m_subTextObjects[i].sharedMaterial == null || m_subTextObjects[i].sharedMaterial.mainTexture.GetInstanceID() != m_materialReferences[i].material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
                        {
                            m_subTextObjects[i].sharedMaterial = m_materialReferences[i].material;
                            m_subTextObjects[i].fontAsset = m_materialReferences[i].fontAsset;
                            m_subTextObjects[i].spriteAsset = m_materialReferences[i].spriteAsset;
                        }
                    }

                    // Check if we need to use a Fallback Material
                    if (m_materialReferences[i].isFallbackMaterial)
                    {
                        m_subTextObjects[i].fallbackMaterial = m_materialReferences[i].material;
                        m_subTextObjects[i].fallbackSourceMaterial = m_materialReferences[i].fallbackMaterial;
                    }

                }

                int referenceCount = m_materialReferences[i].referenceCount;

                // Check to make sure our buffers allocations can accommodate the required text elements.
                if (m_textInfo.meshInfo[i].vertices == null || m_textInfo.meshInfo[i].vertices.Length < referenceCount * 4)
                {
                    if (m_textInfo.meshInfo[i].vertices == null)
                    {
                        if (i == 0)
                            m_textInfo.meshInfo[i] = new TMP_MeshInfo(m_mesh, referenceCount + 1);
                        else
                            m_textInfo.meshInfo[i] = new TMP_MeshInfo(m_subTextObjects[i].mesh, referenceCount + 1);
                    }
                    else
                        m_textInfo.meshInfo[i].ResizeMeshInfo(referenceCount > 1024 ? referenceCount + 256 : Mathf.NextPowerOfTwo(referenceCount));
                }
                else if (m_textInfo.meshInfo[i].vertices.Length - referenceCount * 4 > 1024)
                {
                    // Resize vertex buffers if allocations are excessive.
                    //Debug.Log("Reducing the size of the vertex buffers.");
                    m_textInfo.meshInfo[i].ResizeMeshInfo(referenceCount > 1024 ? referenceCount + 256 : Mathf.Max(Mathf.NextPowerOfTwo(referenceCount), 256));
                }
            }

            //TMP_MaterialManager.CleanupFallbackMaterials();

            // Clean up unused SubMeshes
            for (int i = materialCount; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
            {
                if (i < m_textInfo.meshInfo.Length)
                {
                    m_subTextObjects[i].canvasRenderer.SetMesh(null);

                    // TODO: Figure out a way to handle this without running into Unity's Rebuild loop issue.
                    //m_subTextObjects[i].gameObject.SetActive(false);
                }
            }

            #if PROFILE_ON
            Profiler.EndSample();
            #endif

            return m_totalCharacterCount;
        }


        // Added to sort handle the potential issue with OnWillRenderObject() not getting called when objects are not visible by camera.
        //void OnBecameInvisible()
        //{
        //    if (m_mesh != null)
        //        m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
        //}


        /// <summary>
        /// Update the margin width and height
        /// </summary>
        protected override void ComputeMarginSize()
        {
            if (this.rectTransform != null)
            {
                //Debug.Log("*** ComputeMarginSize() *** Current RectTransform's Width is " + m_rectTransform.rect.width + " and Height is " + m_rectTransform.rect.height); // + " and size delta is "  + m_rectTransform.sizeDelta);

                m_marginWidth = m_rectTransform.rect.width - m_margin.x - m_margin.z;
                m_marginHeight = m_rectTransform.rect.height - m_margin.y - m_margin.w;

                // Update the corners of the RectTransform
                m_RectTransformCorners = GetTextContainerLocalCorners();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            m_havePropertiesChanged = true;
            SetVerticesDirty();
            SetLayoutDirty();
            //Debug.Log("Animation Properties have changed.");
        }


        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            m_canvas = this.canvas;
        }


        protected override void OnTransformParentChanged()
        {
            //Debug.Log("***** OnTransformParentChanged *****");

            base.OnTransformParentChanged();

            m_canvas = this.canvas;

            ComputeMarginSize();
            m_havePropertiesChanged = true;
        }


        protected override void OnRectTransformDimensionsChange()
        {
            //Debug.Log("*** OnRectTransformDimensionsChange() *** ActiveInHierarchy: " + this.gameObject.activeInHierarchy + "  Frame: " + Time.frameCount); 

            // Make sure object is active in Hierarchy
            if (!this.gameObject.activeInHierarchy)
                return;

            ComputeMarginSize();

            UpdateSubObjectPivot();

            SetVerticesDirty();
            SetLayoutDirty();
        }


        /// <summary>
        /// Unity standard function used to check if the transform or scale of the text object has changed.
        /// </summary>
        void LateUpdate()
        {
            if (m_rectTransform.hasChanged)
            {
                // We need to update the SDF scale or possibly regenerate the text object if lossy scale has changed.
                float lossyScaleY = m_rectTransform.lossyScale.y;
                if (!m_havePropertiesChanged && lossyScaleY != m_previousLossyScaleY && m_text != string.Empty && m_text != null)
                {
                    UpdateSDFScale(lossyScaleY);

                    m_previousLossyScaleY = lossyScaleY;
                }
                m_rectTransform.hasChanged = false;
            }

            // Added to handle legacy animation mode.
            if (m_isUsingLegacyAnimationComponent)
            {
                //if (m_havePropertiesChanged)
                m_havePropertiesChanged = true;
                OnPreRenderCanvas();
            }
        }



        // Called just before the Canvas is rendered.
        void OnPreRenderCanvas()
        {
            //Debug.Log("*** OnPreRenderCanvas() *** Frame: " + Time.frameCount); 

            // Make sure object is active and that we have a valid Canvas.
            if (!m_isAwake || !m_ignoreActiveState && !this.IsActive()) return;

            if (m_canvas == null) { m_canvas = this.canvas; if (m_canvas == null) return; }


            // Debug Variables
            loopCountA = 0;
            //loopCountB = 0;
            //loopCountC = 0;
            //loopCountD = 0;
            //loopCountE = 0;

            // Update Margins
            //ComputeMarginSize();

            // Update Mask
            //    if (m_isMaskingEnabled)
            //    {
            //        UpdateMask();
            //    }


            if (m_havePropertiesChanged || m_isLayoutDirty)
            {
                //Debug.Log("Properties have changed!"); // Assigned Material is:" + m_sharedMaterial); // New Text is: " + m_text + ".");

                // Update mesh padding if necessary.
                if (checkPaddingRequired)
                    UpdateMeshPadding();

                // Re-parse the text if the input has changed.
                if (m_isInputParsingRequired || m_isTextTruncated)
                    ParseInputText();

                // Reset Font min / max used with Auto-sizing
                if (m_enableAutoSizing)
                    m_fontSize = Mathf.Clamp(m_fontSize, m_fontSizeMin, m_fontSizeMax);

                m_maxFontSize = m_fontSizeMax;
                m_minFontSize = m_fontSizeMin;
                m_lineSpacingDelta = 0;
                m_charWidthAdjDelta = 0;
                m_recursiveCount = 0;

                m_isCharacterWrappingEnabled = false;
                m_isTextTruncated = false;

                m_havePropertiesChanged = false;
                m_isLayoutDirty = false;
                m_ignoreActiveState = false;

                GenerateTextMesh();
            }
        }



        /// <summary>
        /// This is the main function that is responsible for creating / displaying the text.
        /// </summary>
        protected override void GenerateTextMesh()
        {
            //Debug.Log("*** GenerateTextMesh() ***"); // ***** Frame: " + Time.frameCount); // + ". Point Size: " + m_fontSize + ". Margins are (W) " + m_marginWidth + "  (H) " + m_marginHeight); // ". Iteration Count: " + loopCountA + ".  Min: " + m_minFontSize + "  Max: " + m_maxFontSize + "  Delta: " + (m_maxFontSize - m_minFontSize) + "  Font size is " + m_fontSize); //called for Object with ID " + GetInstanceID()); // Assigned Material is " + m_uiRenderer.GetMaterial().name); // IncludeForMasking " + this.m_IncludeForMasking); // and text is " + m_text);
            //Profiler.BeginSample("TMP Generate Text - Pre");

            // Early exit if no font asset was assigned. This should not be needed since Arial SDF will be assigned by default.
            if (m_fontAsset == null || m_fontAsset.characterDictionary == null)
            {
                Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + this.GetInstanceID());
                return;
            }

            // Clear TextInfo
            if (m_textInfo != null)
                m_textInfo.Clear();

            // Early exit if we don't have any Text to generate.
            if (m_char_buffer == null || m_char_buffer.Length == 0 || m_char_buffer[0] == (char)0)
            {
                // Clear mesh and upload changes to the mesh.
                ClearMesh();

                m_preferredWidth = 0;
                m_preferredHeight = 0;

                // Event indicating the text has been regenerated.
                TMPro_EventManager.ON_TEXT_CHANGED(this);

                return;
            }

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;
            m_currentMaterialIndex = 0;
            m_materialReferenceStack.SetDefault(new MaterialReference(0, m_currentFontAsset, null, m_currentMaterial, m_padding));

            m_currentSpriteAsset = m_spriteAsset;

            // Total character count is computed when the text is parsed.
            int totalCharacterCount = m_totalCharacterCount;

            // Calculate the scale of the font based on selected font size and sampling point size.
            m_fontScale = (m_fontSize / m_currentFontAsset.fontInfo.PointSize);
            // baseScale is calculated based on the font asset assigned to the text object.
            float baseScale = (m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale);
            float currentElementScale = m_fontScale;
            m_fontScaleMultiplier = 1;

            m_currentFontSize = m_fontSize;
            m_sizeStack.SetDefault(m_currentFontSize);
            float fontSizeDelta = 0;

            int charCode = 0; // Holds the character code of the currently being processed character.

            m_style = m_fontStyle; // Set the default style.
            m_fontWeightInternal = (m_style & FontStyles.Bold) == FontStyles.Bold ? 700 : m_fontWeight;
            m_fontWeightStack.SetDefault(m_fontWeightInternal);

            m_lineJustification = m_textAlignment; // Sets the line justification mode to match editor alignment.

            float padding = 0;
            float style_padding = 0; // Extra padding required to accommodate Bold style.
            float bold_xAdvance_multiplier = 1; // Used to increase spacing between character when style is bold.

            m_baselineOffset = 0; // Used by subscript characters.

            // Underline
            bool beginUnderline = false;
            Vector3 underline_start = Vector3.zero; // Used to track where underline starts & ends.
            Vector3 underline_end = Vector3.zero;

            // Strike-through
            bool beginStrikethrough = false;
            Vector3 strikethrough_start = Vector3.zero;
            Vector3 strikethrough_end = Vector3.zero;

            m_fontColor32 = m_fontColor;
            Color32 vertexColor;
            m_htmlColor = m_fontColor32;
            m_colorStack.SetDefault(m_htmlColor);

            // Clear the Style stack.
            m_styleStack.Clear();

            // Clear the Action stack.
            m_actionStack.Clear();

            m_lineOffset = 0; // Amount of space between lines (font line spacing + m_linespacing).
            m_lineHeight = 0;
            float lineGap = m_currentFontAsset.fontInfo.LineHeight - (m_currentFontAsset.fontInfo.Ascender - m_currentFontAsset.fontInfo.Descender);

            m_cSpacing = 0; // Amount of space added between characters as a result of the use of the <cspace> tag.
            m_monoSpacing = 0;
            float lineOffsetDelta = 0;
            m_xAdvance = 0; // Used to track the position of each character.

            tag_LineIndent = 0; // Used for indentation of text.
            tag_Indent = 0;
            m_indentStack.SetDefault(0);
            tag_NoParsing = false;
            //m_isIgnoringAlignment = false;

            m_characterCount = 0; // Total characters in the char[]

            // Tracking of line information
            m_firstCharacterOfLine = 0;
            m_lastCharacterOfLine = 0;
            m_firstVisibleCharacterOfLine = 0;
            m_lastVisibleCharacterOfLine = 0;
            m_maxLineAscender = k_LargeNegativeFloat;
            m_maxLineDescender = k_LargePositiveFloat;
            m_lineNumber = 0;
            m_lineVisibleCharacterCount = 0;
            bool isStartOfNewLine = true;

            m_pageNumber = 0;
            int pageToDisplay = Mathf.Clamp(m_pageToDisplay - 1, 0, m_textInfo.pageInfo.Length - 1);

            int ellipsisIndex = 0;

            Vector4 margins = m_margin;
            float marginWidth = m_marginWidth;
            float marginHeight = m_marginHeight;
            m_marginLeft = 0;
            m_marginRight = 0;
            m_width = -1;
            float width = marginWidth + 0.0001f - m_marginLeft - m_marginRight;

            // Need to initialize these Extents structures
            m_meshExtents.min = k_LargePositiveVector2;
            m_meshExtents.max = k_LargeNegativeVector2;

            // Initialize lineInfo
            m_textInfo.ClearLineInfo();

            // Tracking of the highest Ascender
            m_maxCapHeight = 0;
            m_maxAscender = 0;
            m_maxDescender = 0;
            float pageAscender = 0;
            float maxVisibleDescender = 0;
            bool isMaxVisibleDescenderSet = false;
            m_isNewPage = false;

            // Initialize struct to track states of word wrapping
            bool isFirstWord = true;
            bool isLastBreakingChar = false;
            int wrappingIndex = 0;

            loopCountA += 1;
            //Profiler.EndSample();

            #if PROFILE_PHASES_ON
            Profiler.BeginSample("TMP Generate Text - Phase I");
            #endif

            int endTagIndex = 0;
            // Parse through Character buffer to read HTML tags and begin creating mesh.
            for (int i = 0; m_char_buffer[i] != 0; i++)
            {
                charCode = m_char_buffer[i];
                m_textElementType = TMP_TextElementType.Character;
                m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
                m_currentFontAsset = m_materialReferences[m_currentMaterialIndex].fontAsset;

                int prev_MaterialIndex = m_currentMaterialIndex;

                // Parse Rich Text Tag
                #region Parse Rich Text Tag
                if (m_isRichText && charCode == 60)  // '<'
                {
                    m_isParsingText = true;

                    // Check if Tag is valid. If valid, skip to the end of the validated tag.
                    if (ValidateHtmlTag(m_char_buffer, i + 1, out endTagIndex))
                    {
                        i = endTagIndex;

                        // Continue to next character or handle the sprite element
                        if (m_textElementType == TMP_TextElementType.Character)
                            continue;
                    }
                }
                #endregion End Parse Rich Text Tag

                m_isParsingText = false;

                bool isUsingAltTypeface = m_textInfo.characterInfo[m_characterCount].isUsingAlternateTypeface;


                // Handle Font Styles like LowerCase, UpperCase and SmallCaps.
                #region Handling of LowerCase, UpperCase and SmallCaps Font Styles
#if PROFILE_ON
                Profiler.BeginSample("Handle Font Style");
#endif
                float smallCapsMultiplier = 1.0f;

                if (m_textElementType == TMP_TextElementType.Character)
                {
                    if ((m_style & FontStyles.UpperCase) == FontStyles.UpperCase)
                    {
                        // If this character is lowercase, switch to uppercase.
                        if (char.IsLower((char)charCode))
                            charCode = char.ToUpper((char)charCode);

                    }
                    else if ((m_style & FontStyles.LowerCase) == FontStyles.LowerCase)
                    {
                        // If this character is uppercase, switch to lowercase.
                        if (char.IsUpper((char)charCode))
                            charCode = char.ToLower((char)charCode);
                    }
                    else if ((m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps)
                    {
                        if (char.IsLower((char)charCode))
                        {
                            smallCapsMultiplier = 0.8f;
                            charCode = char.ToUpper((char)charCode);
                        }
                    }
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion


                // Look up Character Data from Dictionary and cache it.
                #region Look up Character Data
                #if PROFILE_ON
                Profiler.BeginSample("Lookup Character Data");
                #endif
                if (m_textElementType == TMP_TextElementType.Sprite)
                {
                    TMP_Sprite sprite = m_currentSpriteAsset.spriteInfoList[m_spriteIndex];
                    if (sprite == null) continue;

                    // Sprites are assigned in the E000 Private Area + sprite Index
                    charCode = 57344 + m_spriteIndex;

                    m_currentFontAsset = m_fontAsset;

                    // The sprite scale calculations are based on the font asset assigned to the text object.
                    float spriteScale = (m_currentFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale);
                    currentElementScale = m_fontAsset.fontInfo.Ascender / sprite.height * sprite.scale * spriteScale;

                    m_cached_TextElement = sprite;

                    m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;
                    m_textInfo.characterInfo[m_characterCount].scale = spriteScale;
                    m_textInfo.characterInfo[m_characterCount].spriteAsset = m_currentSpriteAsset;
                    m_textInfo.characterInfo[m_characterCount].fontAsset = m_currentFontAsset;
                    m_textInfo.characterInfo[m_characterCount].materialReferenceIndex = m_currentMaterialIndex;

                    m_currentMaterialIndex = prev_MaterialIndex;

                    padding = 0;
                }
                else if (m_textElementType == TMP_TextElementType.Character)
                {
                    m_cached_TextElement = m_textInfo.characterInfo[m_characterCount].textElement;
                    if (m_cached_TextElement == null) continue;

                    m_currentFontAsset = m_textInfo.characterInfo[m_characterCount].fontAsset;
                    m_currentMaterial = m_textInfo.characterInfo[m_characterCount].material;
                    m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;

                    // Re-calculate font scale as the font asset may have changed.
                    m_fontScale = m_currentFontSize * smallCapsMultiplier / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale;

                    currentElementScale = m_fontScale * m_fontScaleMultiplier * m_cached_TextElement.scale;

                    m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
                    m_textInfo.characterInfo[m_characterCount].scale = currentElementScale;

                    padding = m_currentMaterialIndex == 0 ? m_padding : m_subTextObjects[m_currentMaterialIndex].padding;
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion


                // Handle Soft Hyphen
                #region Handle Soft Hyphen
                float old_scale = currentElementScale;
                if (charCode == 0xAD)
                {
                    currentElementScale = 0;
                }
                #endregion


                // Initial Implementation for RTL support.
                if (m_isRightToLeft)
                    m_xAdvance -= ((m_cached_TextElement.xAdvance * bold_xAdvance_multiplier + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale + m_cSpacing) * (1 - m_charWidthAdjDelta);


                // Store some of the text object's information
                m_textInfo.characterInfo[m_characterCount].character = (char)charCode;
                m_textInfo.characterInfo[m_characterCount].pointSize = m_currentFontSize;
                m_textInfo.characterInfo[m_characterCount].color = m_htmlColor;
                m_textInfo.characterInfo[m_characterCount].style = m_style;
                m_textInfo.characterInfo[m_characterCount].index = (short)i;
                //m_textInfo.characterInfo[m_characterCount].isIgnoringAlignment = m_isIgnoringAlignment;


                // Handle Kerning if Enabled.
                #region Handle Kerning
                if (m_enableKerning && m_characterCount >= 1)
                {
                    int prev_charCode = m_textInfo.characterInfo[m_characterCount - 1].character;
                    KerningPairKey keyValue = new KerningPairKey(prev_charCode, charCode);

                    KerningPair pair;

                    m_currentFontAsset.kerningDictionary.TryGetValue(keyValue.key, out pair);
                    if (pair != null)
                    {
                        m_xAdvance += pair.XadvanceOffset * currentElementScale;
                    }
                }
                #endregion


                // Handle Mono Spacing
                #region Handle Mono Spacing
                float monoAdvance = 0;
                if (m_monoSpacing != 0)
                {
                    monoAdvance = (m_monoSpacing / 2 - (m_cached_TextElement.width / 2 + m_cached_TextElement.xOffset) * currentElementScale) * (1 - m_charWidthAdjDelta);
                    m_xAdvance += monoAdvance;
                }
                #endregion


                // Set Padding based on selected font style
                #region Handle Style Padding
                if (m_textElementType == TMP_TextElementType.Character && !isUsingAltTypeface && ((m_style & FontStyles.Bold) == FontStyles.Bold || (m_fontStyle & FontStyles.Bold) == FontStyles.Bold)) // Checks for any combination of Bold Style.
                {
                    style_padding = m_currentFontAsset.boldStyle * 2;
                    bold_xAdvance_multiplier = 1 + m_currentFontAsset.boldSpacing * 0.01f;
                }
                else
                {
                    style_padding = m_currentFontAsset.normalStyle * 2;
                    bold_xAdvance_multiplier = 1.0f;
                }
                #endregion Handle Style Padding


                // Determine the position of the vertices of the Character or Sprite.
                #region Calculate Vertices Position
                #if PROFILE_ON
                Profiler.BeginSample("Calculate Vertices Position");
                #endif
                float fontBaseLineOffset = m_currentFontAsset.fontInfo.Baseline;
                Vector3 top_left;
                top_left.x = m_xAdvance + ((m_cached_TextElement.xOffset - padding - style_padding) * currentElementScale * (1 - m_charWidthAdjDelta));
                top_left.y = (fontBaseLineOffset + m_cached_TextElement.yOffset + padding) * currentElementScale - m_lineOffset + m_baselineOffset;
                top_left.z = 0;

                Vector3 bottom_left;
                bottom_left.x = top_left.x;
                bottom_left.y = top_left.y - ((m_cached_TextElement.height + padding * 2) * currentElementScale);
                bottom_left.z = 0;

                Vector3 top_right;
                top_right.x = bottom_left.x + ((m_cached_TextElement.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta));
                top_right.y = top_left.y;
                top_right.z = 0;

                Vector3 bottom_right;
                bottom_right.x = top_right.x;
                bottom_right.y = bottom_left.y;
                bottom_right.z = 0;


                //Vector3 top_left = new Vector3(0 + m_xAdvance + ((m_cached_TextElement.xOffset - padding - style_padding) * currentElementScale * (1 - m_charWidthAdjDelta)), (fontBaseLineOffset + m_cached_TextElement.yOffset + padding) * currentElementScale - m_lineOffset + m_baselineOffset, 0);
                //Vector3 bottom_left = new Vector3(top_left.x, top_left.y - ((m_cached_TextElement.height + padding * 2) * currentElementScale), 0);
                //Vector3 top_right = new Vector3(bottom_left.x + ((m_cached_TextElement.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta)), top_left.y, 0);
                //Vector3 bottom_right = new Vector3(top_right.x, bottom_left.y, 0);
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion

                // Check if we need to Shear the rectangles for Italic styles
                #region Handle Italic & Shearing
                if (m_textElementType == TMP_TextElementType.Character && !isUsingAltTypeface && ((m_style & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic))
                {
                    // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
                    float shear_value = m_currentFontAsset.italicStyle * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * ((m_cached_TextElement.yOffset + padding + style_padding) * currentElementScale), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (((m_cached_TextElement.yOffset - m_cached_TextElement.height - padding - style_padding)) * currentElementScale), 0, 0);

                    top_left = top_left + topShear;
                    bottom_left = bottom_left + bottomShear;
                    top_right = top_right + topShear;
                    bottom_right = bottom_right + bottomShear;
                }
                #endregion Handle Italics & Shearing


                // Store vertex information for the character or sprite.
                m_textInfo.characterInfo[m_characterCount].bottomLeft = bottom_left;
                m_textInfo.characterInfo[m_characterCount].topLeft = top_left;
                m_textInfo.characterInfo[m_characterCount].topRight = top_right;
                m_textInfo.characterInfo[m_characterCount].bottomRight = bottom_right;

                m_textInfo.characterInfo[m_characterCount].origin = m_xAdvance;
                m_textInfo.characterInfo[m_characterCount].baseLine = 0 - m_lineOffset + m_baselineOffset;
                m_textInfo.characterInfo[m_characterCount].aspectRatio = (top_right.x - bottom_left.x) / (top_left.y - bottom_left.y);


                // Compute and save text element Ascender and maximum line Ascender.
                float elementAscender = m_currentFontAsset.fontInfo.Ascender * (m_textElementType == TMP_TextElementType.Character ? currentElementScale : m_textInfo.characterInfo[m_characterCount].scale) + m_baselineOffset;
                m_textInfo.characterInfo[m_characterCount].ascender = elementAscender - m_lineOffset;
                m_maxLineAscender = elementAscender > m_maxLineAscender ? elementAscender : m_maxLineAscender;

                // Compute and save text element Descender and maximum line Descender.
                float elementDescender = m_currentFontAsset.fontInfo.Descender * (m_textElementType == TMP_TextElementType.Character ? currentElementScale : m_textInfo.characterInfo[m_characterCount].scale) + m_baselineOffset;
                float elementDescenderII = m_textInfo.characterInfo[m_characterCount].descender = elementDescender - m_lineOffset;
                m_maxLineDescender = elementDescender < m_maxLineDescender ? elementDescender : m_maxLineDescender;

                // Adjust maxLineAscender and maxLineDescender if style is superscript or subscript
                if ((m_style & FontStyles.Subscript) == FontStyles.Subscript || (m_style & FontStyles.Superscript) == FontStyles.Superscript)
                {
                    float baseAscender = (elementAscender - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
                    elementAscender = m_maxLineAscender;
                    m_maxLineAscender = baseAscender > m_maxLineAscender ? baseAscender : m_maxLineAscender;

                    float baseDescender = (elementDescender - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
                    elementDescender = m_maxLineDescender;
                    m_maxLineDescender = baseDescender < m_maxLineDescender ? baseDescender : m_maxLineDescender;
                }

                if (m_lineNumber == 0)
                {
                    m_maxAscender = m_maxAscender > elementAscender ? m_maxAscender : elementAscender;
                    m_maxCapHeight = Mathf.Max(m_maxCapHeight, m_currentFontAsset.fontInfo.CapHeight * currentElementScale);
                }
                if (m_lineOffset == 0) pageAscender = pageAscender > elementAscender ? pageAscender : elementAscender;


                // Set Characters to not visible by default.
                m_textInfo.characterInfo[m_characterCount].isVisible = false;
                //float width = m_width != -1 ? Mathf.Min(marginWidth + 0.0001f - m_marginLeft - m_marginRight, m_width) : marginWidth + 0.0001f - m_marginLeft - m_marginRight;

                // Setup Mesh for visible text elements. ie. not a SPACE / LINEFEED / CARRIAGE RETURN.
                #region Handle Visible Characters
                //#if PROFILE_ON
                //Profiler.BeginSample("Handle Visible Characters");
                //#endif
                if (charCode == 9 || !char.IsWhiteSpace((char)charCode) || m_textElementType == TMP_TextElementType.Sprite)
                {
                    m_textInfo.characterInfo[m_characterCount].isVisible = true;

                    width = m_width != -1 ? Mathf.Min(marginWidth + 0.0001f - m_marginLeft - m_marginRight, m_width) : marginWidth + 0.0001f - m_marginLeft - m_marginRight;
                    m_textInfo.lineInfo[m_lineNumber].marginLeft = m_marginLeft;

                    // Check if Character exceeds the width of the Text Container
                    #region Handle Line Breaking, Text Auto-Sizing and Horizontal Overflow

                    if (Mathf.Abs(m_xAdvance) + (!m_isRightToLeft ? m_cached_TextElement.xAdvance : 0) * (1 - m_charWidthAdjDelta) * (charCode != 0xAD ? currentElementScale : old_scale) > width)
                    {
                        ellipsisIndex = m_characterCount - 1; // Last safely rendered character

                        // Word Wrapping
                        #region Handle Word Wrapping
                        if (enableWordWrapping && m_characterCount != m_firstCharacterOfLine)
                        {
                            // Check if word wrapping is still possible
                            #region Line Breaking Check
                            if (wrappingIndex == m_SavedWordWrapState.previous_WordBreak || isFirstWord)
                            {
                                // Word wrapping is no longer possible. Shrink size of text if auto-sizing is enabled.
                                if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                                {
                                    // Handle Character Width Adjustments
                                    #region Character Width Adjustments
                                    if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                                    {
                                        loopCountA = 0;
                                        m_charWidthAdjDelta += 0.01f;
                                        GenerateTextMesh();
                                        return;
                                    }
                                    #endregion

                                    // Adjust Point Size
                                    m_maxFontSize = m_fontSize;

                                    m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                    m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                                    if (loopCountA > 20) return; // Added to debug
                                    GenerateTextMesh();
                                    return;
                                }

                                // Word wrapping is no longer possible, now breaking up individual words.
                                if (m_isCharacterWrappingEnabled == false)
                                {
                                    m_isCharacterWrappingEnabled = true;
                                }
                                else
                                    isLastBreakingChar = true;

                                m_recursiveCount += 1;
                                if (m_recursiveCount > 20)
                                {
                                    //Debug.Log("Recursive count exceeded!");
                                    continue;
                                }
                            }
                            #endregion

                            // Restore to previously stored state of last valid (space character or linefeed)
                            i = RestoreWordWrappingState(ref m_SavedWordWrapState);
                            wrappingIndex = i;  // Used to detect when line length can no longer be reduced.

                            // Handling for Soft Hyphen
                            if (m_char_buffer[i] == 0xAD) // && !m_isCharacterWrappingEnabled) // && ellipsisIndex != i && !m_isCharacterWrappingEnabled)
                            {
                                m_isTextTruncated = true;
                                m_char_buffer[i] = 0x2D;
                                GenerateTextMesh();
                                return;
                            }
                            //Debug.Log("Last Visible Character of line # " + m_lineNumber + " is [" + m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].character + " Character Count: " + m_characterCount + " Last visible: " + m_lastVisibleCharacterOfLine);

                            // Check if Line Spacing of previous line needs to be adjusted.
                            if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                            {
                                //Debug.Log("(Line Break - Adjusting Line Spacing on line #" + m_lineNumber);
                                float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                                AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                                m_lineOffset += offsetDelta;
                                m_SavedWordWrapState.lineOffset = m_lineOffset;
                                m_SavedWordWrapState.previousLineAscender = m_maxLineAscender;

                                // TODO - Add check for character exceeding vertical bounds
                            }
                            m_isNewPage = false;

                            // Calculate lineAscender & make sure if last character is superscript or subscript that we check that as well.
                            float lineAscender = m_maxLineAscender - m_lineOffset;
                            float lineDescender = m_maxLineDescender - m_lineOffset;


                            // Update maxDescender and maxVisibleDescender
                            m_maxDescender = m_maxDescender < lineDescender ? m_maxDescender : lineDescender;
                            if (!isMaxVisibleDescenderSet)
                                maxVisibleDescender = m_maxDescender;

                            if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
                                isMaxVisibleDescenderSet = true;

                            // Track & Store lineInfo for the new line
                            m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
                            m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine = m_firstCharacterOfLine > m_firstVisibleCharacterOfLine ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine;
                            m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = m_lastCharacterOfLine = m_characterCount - 1 > 0 ? m_characterCount - 1 : 0;
                            m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine = m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine;

                            m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
                            m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
                            m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, lineDescender);
                            m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, lineAscender);
                            m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x;
                            m_textInfo.lineInfo[m_lineNumber].width = width;

                            m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale - m_cSpacing;

                            m_textInfo.lineInfo[m_lineNumber].baseline = 0 - m_lineOffset;
                            m_textInfo.lineInfo[m_lineNumber].ascender = lineAscender;
                            m_textInfo.lineInfo[m_lineNumber].descender = lineDescender;
                            m_textInfo.lineInfo[m_lineNumber].lineHeight = lineAscender - lineDescender + lineGap * baseScale;

                            m_firstCharacterOfLine = m_characterCount; // Store first character of the next line.
                            m_lineVisibleCharacterCount = 0;

                            // Store the state of the line before starting on the new line.
                            SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount - 1);

                            m_lineNumber += 1;
                            isStartOfNewLine = true;

                            // Check to make sure Array is large enough to hold a new line.
                            if (m_lineNumber >= m_textInfo.lineInfo.Length)
                                ResizeLineExtents(m_lineNumber);

                            // Apply Line Spacing based on scale of the last character of the line.
                            if (m_lineHeight == 0)
                            {
                                float ascender = m_textInfo.characterInfo[m_characterCount].ascender - m_textInfo.characterInfo[m_characterCount].baseLine;
                                lineOffsetDelta = 0 - m_maxLineDescender + ascender + (lineGap + m_lineSpacing + m_lineSpacingDelta) * baseScale;
                                m_lineOffset += lineOffsetDelta;

                                m_startOfLineAscender = ascender;
                            }
                            else
                                m_lineOffset += m_lineHeight + m_lineSpacing * baseScale;

                            m_maxLineAscender = k_LargeNegativeFloat;
                            m_maxLineDescender = k_LargePositiveFloat;

                            m_xAdvance = 0 + tag_Indent;

                            continue;
                        }
                        #endregion End Word Wrapping


                        // Text Auto-Sizing (text exceeding Width of container. 
                        #region Handle Text Auto-Sizing
                        if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                        {
                            // Handle Character Width Adjustments
                            #region Character Width Adjustments
                            if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                            {
                                loopCountA = 0;
                                m_charWidthAdjDelta += 0.01f;
                                GenerateTextMesh();
                                return;
                            }
                            #endregion

                            // Adjust Point Size
                            m_maxFontSize = m_fontSize;

                            m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                            m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                            m_recursiveCount = 0;
                            if (loopCountA > 20) return; // Added to debug 
                            GenerateTextMesh();
                            return;
                        }
                        #endregion End Text Auto-Sizing


                        // Handle Text Overflow
                        #region Handle Text Overflow
                        switch (m_overflowMode)
                        {
                            case TextOverflowModes.Overflow:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                break;
                            case TextOverflowModes.Ellipsis:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                m_isTextTruncated = true;

                                if (m_characterCount < 1)
                                {
                                    m_textInfo.characterInfo[m_characterCount].isVisible = false;
                                    //m_visibleCharacterCount = 0;
                                    break;
                                }

                                m_char_buffer[i - 1] = 8230;
                                m_char_buffer[i] = (char)0;

                                if (m_cached_Ellipsis_GlyphInfo != null)
                                {
                                    m_textInfo.characterInfo[ellipsisIndex].character = (char)8230;
                                    m_textInfo.characterInfo[ellipsisIndex].textElement = m_cached_Ellipsis_GlyphInfo;
                                    m_textInfo.characterInfo[ellipsisIndex].fontAsset = m_materialReferences[0].fontAsset;
                                    m_textInfo.characterInfo[ellipsisIndex].material = m_materialReferences[0].material;
                                    m_textInfo.characterInfo[ellipsisIndex].materialReferenceIndex = 0;
                                }
                                else
                                {
                                    Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", this);
                                }

                                m_totalCharacterCount = ellipsisIndex + 1;

                                GenerateTextMesh();
                                return;
                            case TextOverflowModes.Masking:
                                if (!m_isMaskingEnabled)
                                    EnableMasking();
                                break;
                            case TextOverflowModes.ScrollRect:
                                if (!m_isMaskingEnabled)
                                    EnableMasking();
                                break;
                            case TextOverflowModes.Truncate:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                m_textInfo.characterInfo[m_characterCount].isVisible = false;
                                break;
                        }
                        #endregion End Text Overflow

                    }
                    #endregion End Check for Characters Exceeding Width of Text Container

                    if (charCode != 9)
                    {
                        // Determine Vertex Color : TODO: Add special handling for sprites where we want to control if they are tinting with the vertex color
                        if (m_overrideHtmlColors)
                            vertexColor = m_fontColor32;
                        else
                            vertexColor = m_htmlColor;

                        // Store Character & Sprite Vertex Information
                        if (m_textElementType == TMP_TextElementType.Character)
                        {
                            // Save Character Vertex Data
                            SaveGlyphVertexInfo(padding, style_padding, vertexColor);
                        }
                        else if (m_textElementType == TMP_TextElementType.Sprite)
                        {
                            SaveSpriteVertexInfo(vertexColor);
                        }
                    }
                    else // If character is Tab
                    {
                        m_textInfo.characterInfo[m_characterCount].isVisible = false;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                        m_textInfo.lineInfo[m_lineNumber].spaceCount += 1;
                        m_textInfo.spaceCount += 1;
                    }

                    // Increase visible count for Characters.
                    if (m_textInfo.characterInfo[m_characterCount].isVisible && charCode != 0xAD)
                    {
                        if (isStartOfNewLine) { isStartOfNewLine = false; m_firstVisibleCharacterOfLine = m_characterCount; }

                        m_lineVisibleCharacterCount += 1;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                    }
                }
                else
                {   // This is a Space, Tab, LineFeed or Carriage Return

                    // Track # of spaces per line which is used for line justification.
                    if ((charCode == 10 || char.IsSeparator((char)charCode)) && charCode != 0xAD && charCode != 0x200B && charCode != 0x2060)
                    {
                        m_textInfo.lineInfo[m_lineNumber].spaceCount += 1;
                        m_textInfo.spaceCount += 1;
                    }
                }
                //#if PROFILE_ON
                //Profiler.EndSample();
                //#endif
                #endregion Handle Visible Characters


                // Check if Line Spacing of previous line needs to be adjusted.
                #region Adjust Line Spacing
                #if PROFILE_ON
                Profiler.BeginSample("Adjust Line Spacing");
                #endif
                if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                {
                    //Debug.Log("Inline - Adjusting Line Spacing on line #" + m_lineNumber);
                    //float gap = 0; // Compute gap.

                    float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                    AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                    elementDescenderII -= offsetDelta;
                    m_lineOffset += offsetDelta;

                    m_startOfLineAscender += offsetDelta;
                    m_SavedWordWrapState.lineOffset = m_lineOffset;
                    m_SavedWordWrapState.previousLineAscender = m_startOfLineAscender;
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion


                // Store Rectangle positions for each Character.
                #region Store Character Data
                m_textInfo.characterInfo[m_characterCount].lineNumber = (short)m_lineNumber;
                m_textInfo.characterInfo[m_characterCount].pageNumber = (short)m_pageNumber;

                if (charCode != 10 && charCode != 13 && charCode != 8230 || m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
                    m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
                #endregion Store Character Data


                // Check if text Exceeds the vertical bounds of the margin area.
                #region Check Vertical Bounds & Auto-Sizing
                #if PROFILE_ON
                Profiler.BeginSample("Check Vertical Bounds");
                #endif
                if (m_maxAscender - elementDescenderII > marginHeight + 0.0001f)
                {
                    // Handle Line spacing adjustments
                    #region Line Spacing Adjustments
                    if (m_enableAutoSizing && m_lineSpacingDelta > m_lineSpacingMax && m_lineNumber > 0)
                    {
                        loopCountA = 0;

                        m_lineSpacingDelta -= 1;
                        GenerateTextMesh();
                        return;
                    }
                    #endregion


                    // Handle Text Auto-sizing resulting from text exceeding vertical bounds.
                    #region Text Auto-Sizing (Text greater than vertical bounds)
                    if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                    {
                        m_maxFontSize = m_fontSize;

                        m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                        m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                        m_recursiveCount = 0;
                        if (loopCountA > 20) return; // Added to debug 
                        GenerateTextMesh();
                        return;
                    }
                    #endregion Text Auto-Sizing


                    // Handle Text Overflow
                    #region Text Overflow
                    switch (m_overflowMode)
                    {
                        case TextOverflowModes.Overflow:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            break;
                        case TextOverflowModes.Ellipsis:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            if (m_lineNumber > 0)
                            {
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index] = 8230;
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index + 1] = (char)0;

                                if (m_cached_Ellipsis_GlyphInfo != null)
                                {
                                    m_textInfo.characterInfo[ellipsisIndex].character = (char)8230;
                                    m_textInfo.characterInfo[ellipsisIndex].textElement = m_cached_Ellipsis_GlyphInfo;
                                    m_textInfo.characterInfo[ellipsisIndex].fontAsset = m_materialReferences[0].fontAsset;
                                    m_textInfo.characterInfo[ellipsisIndex].material = m_materialReferences[0].material;
                                    m_textInfo.characterInfo[ellipsisIndex].materialReferenceIndex = 0;
                                }
                                else
                                {
                                    Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", this);
                                }

                                m_totalCharacterCount = ellipsisIndex + 1;

                                GenerateTextMesh();
                                m_isTextTruncated = true;
                                return;
                            }
                            else
                            {
                                ClearMesh();
                                return;
                            }
                        case TextOverflowModes.Masking:
                            if (!m_isMaskingEnabled)
                                EnableMasking();
                            break;
                        case TextOverflowModes.ScrollRect:
                            if (!m_isMaskingEnabled)
                                EnableMasking();
                            break;
                        case TextOverflowModes.Truncate:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            // TODO : Optimize 
                            if (m_lineNumber > 0)
                            {
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index + 1] = (char)0;

                                m_totalCharacterCount = ellipsisIndex + 1;

                                GenerateTextMesh();
                                m_isTextTruncated = true;
                                return;
                            }
                            else
                            {
                                ClearMesh();
                                return;
                            }
                        case TextOverflowModes.Page:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            // Ignore Page Break, Linefeed or carriage return
                            if (charCode == 13 || charCode == 10)
                                break;

                            // Go back to previous line and re-layout 
                            i = RestoreWordWrappingState(ref m_SavedLineState);
                            if (i == 0)
                            {
                                ClearMesh();

                                return;
                            }

                            m_isNewPage = true;
                            m_xAdvance = 0 + tag_Indent;
                            m_lineOffset = 0;
                            m_lineNumber += 1;
                            m_pageNumber += 1;
                            continue;
                    }
                    #endregion End Text Overflow

                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion Check Vertical Bounds


                // Handle xAdvance & Tabulation Stops. Tab stops at every 25% of Font Size.
                #region XAdvance, Tabulation & Stops
                if (charCode == 9)
                {
                    float tabSize = m_currentFontAsset.fontInfo.TabWidth * currentElementScale;
                    float tabs = Mathf.Ceil(m_xAdvance / tabSize) * tabSize;
                    m_xAdvance = tabs > m_xAdvance ? tabs : m_xAdvance + tabSize;
                }
                else if (m_monoSpacing != 0)
                    m_xAdvance += (m_monoSpacing - monoAdvance + ((m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale) + m_cSpacing) * (1 - m_charWidthAdjDelta);
                else if (!m_isRightToLeft)
                {
                    m_xAdvance += ((m_cached_TextElement.xAdvance * bold_xAdvance_multiplier + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale + m_cSpacing) * (1 - m_charWidthAdjDelta);
                }


                // Store xAdvance information
                m_textInfo.characterInfo[m_characterCount].xAdvance = m_xAdvance;

                #endregion Tabulation & Stops


                // Handle Carriage Return
                #region Carriage Return
                if (charCode == 13)
                {
                    m_xAdvance = 0 + tag_Indent;
                }
                #endregion Carriage Return


                // Handle Line Spacing Adjustments + Word Wrapping & special case for last line.
                #region Check for Line Feed and Last Character
                #if PROFILE_ON
                Profiler.BeginSample("Process Linefeed");
                #endif
                if (charCode == 10 || m_characterCount == totalCharacterCount - 1)
                {
                    // Check if Line Spacing of previous line needs to be adjusted.
                    if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                    {
                        //Debug.Log("Line Feed - Adjusting Line Spacing on line #" + m_lineNumber);
                        float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                        AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                        elementDescenderII -= offsetDelta;
                        m_lineOffset += offsetDelta;
                    }
                    m_isNewPage = false;

                    // Calculate lineAscender & make sure if last character is superscript or subscript that we check that as well.
                    float lineAscender = m_maxLineAscender - m_lineOffset;
                    float lineDescender = m_maxLineDescender - m_lineOffset;

                    // Update maxDescender and maxVisibleDescender
                    m_maxDescender = m_maxDescender < lineDescender ? m_maxDescender : lineDescender;
                    if (!isMaxVisibleDescenderSet)
                        maxVisibleDescender = m_maxDescender;

                    if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
                        isMaxVisibleDescenderSet = true;

                    // Save Line Information
                    m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
                    m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine = m_firstCharacterOfLine > m_firstVisibleCharacterOfLine ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine;
                    m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = m_lastCharacterOfLine = m_characterCount;
                    m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine = m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine;

                    m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
                    m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
                    m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, lineDescender);
                    m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, lineAscender);
                    m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x - (padding * currentElementScale);
                    m_textInfo.lineInfo[m_lineNumber].width = width;

                    if (m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
                        m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;

                    if (m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].isVisible)
                        m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale - m_cSpacing;
                    else
                        m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale - m_cSpacing;

                    m_textInfo.lineInfo[m_lineNumber].baseline = 0 - m_lineOffset;
                    m_textInfo.lineInfo[m_lineNumber].ascender = lineAscender;
                    m_textInfo.lineInfo[m_lineNumber].descender = lineDescender;
                    m_textInfo.lineInfo[m_lineNumber].lineHeight = lineAscender - lineDescender + lineGap * baseScale;

                    m_firstCharacterOfLine = m_characterCount + 1;
                    m_lineVisibleCharacterCount = 0;

                    // Add new line if not last lines or character.
                    if (charCode == 10)
                    {
                        // Store the state of the line before starting on the new line.
                        SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount);
                        // Store the state of the last Character before the new line.
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);

                        m_lineNumber += 1;
                        isStartOfNewLine = true;

                        // Check to make sure Array is large enough to hold a new line.
                        if (m_lineNumber >= m_textInfo.lineInfo.Length)
                            ResizeLineExtents(m_lineNumber);

                        // Apply Line Spacing
                        if (m_lineHeight == 0)
                        {
                            lineOffsetDelta = 0 - m_maxLineDescender + elementAscender + (lineGap + m_lineSpacing + m_paragraphSpacing + m_lineSpacingDelta) * baseScale;
                            m_lineOffset += lineOffsetDelta;
                        }
                        else
                            m_lineOffset += m_lineHeight + (m_lineSpacing + m_paragraphSpacing) * baseScale;

                        m_maxLineAscender = k_LargeNegativeFloat;
                        m_maxLineDescender = k_LargePositiveFloat;
                        m_startOfLineAscender = elementAscender;

                        m_xAdvance = 0 + tag_LineIndent + tag_Indent;

                        ellipsisIndex = m_characterCount - 1;

                        m_characterCount += 1;
                        continue;
                    }
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion Check for Linefeed or Last Character


                // Store Rectangle positions for each Character.
                #region Save CharacterInfo for the current character.
                #if PROFILE_ON
                Profiler.BeginSample("Save CharacterInfo & Extents");
                #endif
                // Determine the bounds of the Mesh.
                if (m_textInfo.characterInfo[m_characterCount].isVisible)
                {
                    m_meshExtents.min.x = Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[m_characterCount].bottomLeft.x);
                    m_meshExtents.min.y = Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[m_characterCount].bottomLeft.y);

                    m_meshExtents.max.x = Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[m_characterCount].topRight.x);
                    m_meshExtents.max.y = Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[m_characterCount].topRight.y);

                    //m_meshExtents.min = new Vector2(Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[m_characterCount].bottomLeft.x), Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[m_characterCount].bottomLeft.y));
                    //m_meshExtents.max = new Vector2(Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[m_characterCount].topRight.x), Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[m_characterCount].topRight.y));
                }


                // Save pageInfo Data
                if (m_overflowMode == TextOverflowModes.Page && charCode != 13 && charCode != 10 && m_pageNumber < 16)
                {
                    m_textInfo.pageInfo[m_pageNumber].ascender = pageAscender;
                    m_textInfo.pageInfo[m_pageNumber].descender = elementDescender < m_textInfo.pageInfo[m_pageNumber].descender ? elementDescender : m_textInfo.pageInfo[m_pageNumber].descender;

                    if (m_pageNumber == 0 && m_characterCount == 0)
                        m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
                    else if (m_characterCount > 0 && m_pageNumber != m_textInfo.characterInfo[m_characterCount - 1].pageNumber)
                    {
                        m_textInfo.pageInfo[m_pageNumber - 1].lastCharacterIndex = m_characterCount - 1;
                        m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
                    }
                    else if (m_characterCount == totalCharacterCount - 1)
                        m_textInfo.pageInfo[m_pageNumber].lastCharacterIndex = m_characterCount;
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion Saving CharacterInfo


                // Save State of Mesh Creation for handling of Word Wrapping
                #region Save Word Wrapping State
                #if PROFILE_ON
                Profiler.BeginSample("Save Word Wrapping State");
                #endif
                if (m_enableWordWrapping || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
                {
                    if ((char.IsWhiteSpace((char)charCode) || charCode == 0x2D || charCode == 0xAD) && !m_isNonBreakingSpace && charCode != 0xA0 && charCode != 0x2011 && charCode != 0x202F && charCode != 0x2060)
                    {
                        // We store the state of numerous variables for the most recent Space, LineFeed or Carriage Return to enable them to be restored 
                        // for Word Wrapping.
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                        m_isCharacterWrappingEnabled = false;
                        isFirstWord = false;
                    }
                    // Handling for East Asian languages
                    else if ((  charCode > 0x1100 && charCode < 0x11ff || /* Hangul Jamo */
                                charCode > 0x2E80 && charCode < 0x9FFF || /* CJK */
                                charCode > 0xA960 && charCode < 0xA97F || /* Hangul Jame Extended-A */
                                charCode > 0xAC00 && charCode < 0xD7FF || /* Hangul Syllables */
                                charCode > 0xF900 && charCode < 0xFAFF || /* CJK Compatibility Ideographs */
                                charCode > 0xFE30 && charCode < 0xFE4F || /* CJK Compatibility Forms */
                                charCode > 0xFF00 && charCode < 0xFFEF)   /* CJK Halfwidth */
                                && !m_isNonBreakingSpace)
                    {
                        if (isFirstWord || isLastBreakingChar || TMP_Settings.linebreakingRules.leadingCharacters.ContainsKey(charCode) == false &&
                            (m_characterCount < totalCharacterCount - 1 &&
                            TMP_Settings.linebreakingRules.followingCharacters.ContainsKey(m_textInfo.characterInfo[m_characterCount + 1].character) == false))
                        {
                            SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                            m_isCharacterWrappingEnabled = false;
                            isFirstWord = false;
                        }
                    }
                    else if ((isFirstWord || m_isCharacterWrappingEnabled == true || isLastBreakingChar))
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion Save Word Wrapping State

                m_characterCount += 1;
            }

            // Check Auto Sizing and increase font size to fill text container.
            #region Check Auto-Sizing (Upper Font Size Bounds)
            fontSizeDelta = m_maxFontSize - m_minFontSize;
            if (!m_isCharacterWrappingEnabled && m_enableAutoSizing && fontSizeDelta > 0.051f && m_fontSize < m_fontSizeMax)
            {
                m_minFontSize = m_fontSize;
                m_fontSize += Mathf.Max((m_maxFontSize - m_fontSize) / 2, 0.05f);
                m_fontSize = (int)(Mathf.Min(m_fontSize, m_fontSizeMax) * 20 + 0.5f) / 20f;

                //Debug.Log(m_fontSize);

                if (loopCountA > 20) return; // Added to debug
                GenerateTextMesh();
                return;
            }
            #endregion End Auto-sizing Check


            m_isCharacterWrappingEnabled = false;

            #if PROFILE_PHASES_ON
                Profiler.EndSample();
            #endif

            //Debug.Log("Iteration Count: " + loopCountA + ". Final Point Size: " + m_fontSize); // + "  B: " + loopCountB + "  C: " + loopCountC + "  D: " + loopCountD);

            // *** PHASE II of Text Generation ***
            #if PROFILE_PHASES_ON
                Profiler.BeginSample("TMP Generate Text - Phase II");
            #endif

            // If there are no visible characters... no need to continue
            if (m_characterCount == 0) // && m_visibleSpriteCount == 0)
            {
                ClearMesh();

                // Event indicating the text has been regenerated.
                TMPro_EventManager.ON_TEXT_CHANGED(this);

                return;
            }

            int last_vert_index = m_materialReferences[0].referenceCount * 4;

            // Partial clear of the vertices array to mark unused vertices as degenerate.
            m_textInfo.meshInfo[0].Clear(false); // ClearUnusedVertices(last_vert_index, false);

            // Handle Text Alignment
            #region Text Vertical Alignment
            #if PROFILE_ON
            Profiler.BeginSample("Vertical Text Alignment");
            #endif
            Vector3 anchorOffset = Vector3.zero;
            Vector3[] corners = m_RectTransformCorners; // GetTextContainerLocalCorners();

            switch (m_textAlignment)
            {
                // Top Vertically
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.TopJustified:
                case TextAlignmentOptions.TopRight:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = corners[1] + new Vector3(0 + margins.x, 0 - m_maxAscender - margins.y, 0);
                    else
                        anchorOffset = corners[1] + new Vector3(0 + margins.x, 0 - m_textInfo.pageInfo[pageToDisplay].ascender - margins.y, 0);
                    break;

                // Middle Vertically
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.Center:
                case TextAlignmentOptions.Justified:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_maxAscender + margins.y + maxVisibleDescender - margins.w) / 2, 0);
                    else
                        anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_textInfo.pageInfo[pageToDisplay].ascender + margins.y + m_textInfo.pageInfo[pageToDisplay].descender - margins.w) / 2, 0);
                    break;

                // Bottom Vertically
                case TextAlignmentOptions.Bottom:
                case TextAlignmentOptions.BottomLeft:
                case TextAlignmentOptions.BottomRight:
                case TextAlignmentOptions.BottomJustified:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = corners[0] + new Vector3(0 + margins.x, 0 - maxVisibleDescender + margins.w, 0);
                    else
                        anchorOffset = corners[0] + new Vector3(0 + margins.x, 0 - m_textInfo.pageInfo[pageToDisplay].descender + margins.w, 0);
                    break;

                // Baseline Vertically
                case TextAlignmentOptions.Baseline:
                case TextAlignmentOptions.BaselineLeft:
                case TextAlignmentOptions.BaselineRight:
                case TextAlignmentOptions.BaselineJustified:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0, 0);
                    break;

                // Midline Vertically 
                case TextAlignmentOptions.MidlineLeft:
                case TextAlignmentOptions.Midline:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.MidlineJustified:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_meshExtents.max.y + margins.y + m_meshExtents.min.y - margins.w) / 2, 0);
                    break;

                // Capline Vertically 
                case TextAlignmentOptions.CaplineLeft:
                case TextAlignmentOptions.Capline:
                case TextAlignmentOptions.CaplineRight:
                case TextAlignmentOptions.CaplineJustified:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_maxCapHeight - margins.y - margins.w) / 2, 0);
                    break;
            }
            #if PROFILE_ON
            Profiler.EndSample();
            #endif
            #endregion


            // Initialization for Second Pass
            Vector3 justificationOffset = Vector3.zero;
            Vector3 offset = Vector3.zero;
            int vert_index_X4 = 0;
            int sprite_index_X4 = 0;

            int wordCount = 0;
            int lineCount = 0;
            int lastLine = 0;

            bool isStartOfWord = false;
            int wordFirstChar = 0;
            int wordLastChar = 0;

            // Second Pass : Line Justification, UV Mapping, Character & Line Visibility & more.
            #region Handle Line Justification & UV Mapping & Character Visibility & More

            // Variables used to handle Canvas Render Modes and SDF Scaling
            bool isCameraAssigned = m_canvas.worldCamera == null ? false : true;
            float lossyScale = m_previousLossyScaleY = this.transform.lossyScale.y;
            RenderMode canvasRenderMode = m_canvas.renderMode;
            float canvasScaleFactor = m_canvas.scaleFactor;

            Color32 underlineColor = Color.white;
            Color32 strikethroughColor = Color.white;
            float xScale = 0;
            float underlineStartScale = 0;
            float underlineEndScale = 0;
            float underlineMaxScale = 0;
            float underlineBaseLine = k_LargePositiveFloat;
            int lastPage = 0;

            float strikethroughPointSize = 0;
            float strikethroughScale = 0;
            float strikethroughBaseline = 0;

            TMP_CharacterInfo[] characterInfos = m_textInfo.characterInfo;
            for (int i = 0; i < m_characterCount; i++)
            {
                char currentCharacter = characterInfos[i].character;

                int currentLine = characterInfos[i].lineNumber;
                TMP_LineInfo lineInfo = m_textInfo.lineInfo[currentLine];
                lineCount = currentLine + 1;

                TextAlignmentOptions lineAlignment = lineInfo.alignment;

                // Process Line Justification
                #region Handle Line Justification
                #if PROFILE_ON
                Profiler.BeginSample("Horizontal Text Alignment");
                #endif 
                //if (!characterInfos[i].isIgnoringAlignment)
                //{
                switch (lineAlignment)
                {
                    case TextAlignmentOptions.TopLeft:
                    case TextAlignmentOptions.Left:
                    case TextAlignmentOptions.BottomLeft:
                    case TextAlignmentOptions.BaselineLeft:
                    case TextAlignmentOptions.MidlineLeft:
                    case TextAlignmentOptions.CaplineLeft:
                        if (!m_isRightToLeft)
                            justificationOffset = new Vector3(0 + lineInfo.marginLeft, 0, 0);
                        else
                            justificationOffset = new Vector3(0 - lineInfo.maxAdvance, 0, 0);
                        break;

                    case TextAlignmentOptions.Top:
                    case TextAlignmentOptions.Center:
                    case TextAlignmentOptions.Bottom:
                    case TextAlignmentOptions.Baseline:
                    case TextAlignmentOptions.Midline:
                    case TextAlignmentOptions.Capline:
                        justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width / 2 - lineInfo.maxAdvance / 2, 0, 0);
                        break;

                    case TextAlignmentOptions.TopRight:
                    case TextAlignmentOptions.Right:
                    case TextAlignmentOptions.BottomRight:
                    case TextAlignmentOptions.BaselineRight:
                    case TextAlignmentOptions.MidlineRight:
                    case TextAlignmentOptions.CaplineRight:
                        if (!m_isRightToLeft)
                            justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width - lineInfo.maxAdvance, 0, 0);
                        else
                            justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width, 0, 0);
                        break;

                    case TextAlignmentOptions.TopJustified:
                    case TextAlignmentOptions.Justified:
                    case TextAlignmentOptions.BottomJustified:
                    case TextAlignmentOptions.BaselineJustified:
                    case TextAlignmentOptions.MidlineJustified:
                    case TextAlignmentOptions.CaplineJustified:
                        // Skip Zero Width Characters
                        if (currentCharacter == 0xAD || currentCharacter == 0x200B || currentCharacter == 0x2060) break;

                        char lastCharOfCurrentLine = characterInfos[lineInfo.lastCharacterIndex].character;

                        if (!char.IsControl(lastCharOfCurrentLine) && currentLine < m_lineNumber)
                        {   
                            // All lines are justified accept the last one.
                            float gap = !m_isRightToLeft ? lineInfo.width - lineInfo.maxAdvance : lineInfo.width + lineInfo.maxAdvance;
                            float ratio = lineInfo.spaceCount > 2 ? m_wordWrappingRatios : 1;

                            if (currentLine != lastLine || i == 0)
                            {
                                if (!m_isRightToLeft)
                                    justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0);
                                else
                                    justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width, 0, 0); // Not sure yet
                            }
                            else
                            {
                                if (currentCharacter == 9 || char.IsSeparator((char)currentCharacter))
                                {
                                    int spaces = characterInfos[lineInfo.lastCharacterIndex].isVisible ? lineInfo.spaceCount : lineInfo.spaceCount - 1;
                                    if (spaces < 1) spaces = 1;

                                    if (!m_isRightToLeft)
                                        justificationOffset += new Vector3(gap * (1 - ratio) / spaces, 0, 0);
                                    else
                                        justificationOffset -= new Vector3(gap * (1 - ratio) / spaces, 0, 0);
                                }
                                else
                                {
                                    if (!m_isRightToLeft)
                                        justificationOffset += new Vector3(gap * ratio / (lineInfo.visibleCharacterCount - 1), 0, 0);
                                    else
                                        justificationOffset -= new Vector3(gap * ratio / (lineInfo.visibleCharacterCount - 1), 0, 0);
                                }
                            }
                        }
                        else
                        {
                            if (!m_isRightToLeft)
                                justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0); // Keep last line left justified.
                            else
                                justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width, 0, 0); // Keep last line right justified.
                        }
                        //Debug.Log("Char [" + (char)charCode + "] Code:" + charCode + "  Line # " + currentLine + "  Offset:" + justificationOffset + "  # Spaces:" + lineInfo.spaceCount + "  # Characters:" + lineInfo.characterCount);
                        break;
                }
                //}
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion End Text Justification

                offset = anchorOffset + justificationOffset;

                // Handle Visible Characters
                #region Handle Visible Characters

                bool isCharacterVisible = characterInfos[i].isVisible;
                if (isCharacterVisible)
                {
                    TMP_TextElementType elementType = characterInfos[i].elementType;
                    switch (elementType)
                    {
                        // CHARACTERS
                        case TMP_TextElementType.Character:
                            Extents lineExtents = lineInfo.lineExtents;
                            float uvOffset = (m_uvLineOffset * currentLine) % 1 + m_uvOffset.x;

                            // Setup UV2 based on Character Mapping Options Selected
                            #region Handle UV Mapping Options
                            #if PROFILE_ON
                            Profiler.BeginSample("UV MAPPING");
                            #endif
                            switch (m_horizontalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    characterInfos[i].vertex_BL.uv2.x = 0 + m_uvOffset.x;
                                    characterInfos[i].vertex_TL.uv2.x = 0 + m_uvOffset.x;
                                    characterInfos[i].vertex_TR.uv2.x = 1 + m_uvOffset.x;
                                    characterInfos[i].vertex_BR.uv2.x = 1 + m_uvOffset.x;
                                    break;

                                case TextureMappingOptions.Line:
                                    if (m_textAlignment != TextAlignmentOptions.Justified)
                                    {
                                        characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        break;
                                    }
                                    else // Special Case if Justified is used in Line Mode.
                                    {
                                        characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        break;
                                    }

                                case TextureMappingOptions.Paragraph:
                                    characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    break;

                                case TextureMappingOptions.MatchAspect:

                                    switch (m_verticalMapping)
                                    {
                                        case TextureMappingOptions.Character:
                                            characterInfos[i].vertex_BL.uv2.y = 0 + m_uvOffset.y;
                                            characterInfos[i].vertex_TL.uv2.y = 1 + m_uvOffset.y;
                                            characterInfos[i].vertex_TR.uv2.y = 0 + m_uvOffset.y;
                                            characterInfos[i].vertex_BR.uv2.y = 1 + m_uvOffset.y;
                                            break;

                                        case TextureMappingOptions.Line:
                                            characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                            characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                            break;

                                        case TextureMappingOptions.Paragraph:
                                            characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                            characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                            break;

                                        case TextureMappingOptions.MatchAspect:
                                            Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
                                            break;
                                    }

                                    //float xDelta = 1 - (_uv2s[vert_index + 0].y * textMeshCharacterInfo[i].AspectRatio); // Left aligned
                                    float xDelta = (1 - ((characterInfos[i].vertex_BL.uv2.y + characterInfos[i].vertex_TL.uv2.y) * characterInfos[i].aspectRatio)) / 2; // Center of Rectangle

                                    characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.uv2.y * characterInfos[i].aspectRatio) + xDelta + uvOffset;
                                    characterInfos[i].vertex_TL.uv2.x = characterInfos[i].vertex_BL.uv2.x;
                                    characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TL.uv2.y * characterInfos[i].aspectRatio) + xDelta + uvOffset;
                                    characterInfos[i].vertex_BR.uv2.x = characterInfos[i].vertex_TR.uv2.x;
                                    break;
                            }

                            switch (m_verticalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    characterInfos[i].vertex_BL.uv2.y = 0 + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = 1 + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = 1 + m_uvOffset.y;
                                    characterInfos[i].vertex_BR.uv2.y = 0 + m_uvOffset.y;
                                    break;

                                case TextureMappingOptions.Line:
                                    characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender) + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    break;

                                case TextureMappingOptions.Paragraph:
                                    characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    break;

                                case TextureMappingOptions.MatchAspect:
                                    float yDelta = (1 - ((characterInfos[i].vertex_BL.uv2.x + characterInfos[i].vertex_TR.uv2.x) / characterInfos[i].aspectRatio)) / 2; // Center of Rectangle

                                    characterInfos[i].vertex_BL.uv2.y = yDelta + (characterInfos[i].vertex_BL.uv2.x / characterInfos[i].aspectRatio) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = yDelta + (characterInfos[i].vertex_TR.uv2.x / characterInfos[i].aspectRatio) + m_uvOffset.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    break;
                            }
                            #if PROFILE_ON
                            Profiler.EndSample();
                            #endif
                            #endregion End UV Mapping Options

                            // Pack UV's so that we can pass Xscale needed for Shader to maintain 1:1 ratio.
                            #region Pack Scale into UV2
                            #if PROFILE_ON
                            Profiler.BeginSample("Pack UV");
                            #endif
                            xScale = characterInfos[i].scale * (1 - m_charWidthAdjDelta);
                            if (!characterInfos[i].isUsingAlternateTypeface && (characterInfos[i].style & FontStyles.Bold) == FontStyles.Bold) xScale *= -1;

                            switch (canvasRenderMode)
                            {
                                case RenderMode.ScreenSpaceOverlay:
                                    xScale *= lossyScale / canvasScaleFactor;
                                    break;
                                case RenderMode.ScreenSpaceCamera:
                                    xScale *= isCameraAssigned ? lossyScale : 1;
                                    break;
                                case RenderMode.WorldSpace:
                                    xScale *= lossyScale;
                                    break;
                            }

                            // isBold is encoded in the X value and SDF Scale in Y.
                            //Vector2 vertexData = new Vector2((characterInfos[i].style & FontStyles.Bold) == FontStyles.Bold ? 1 : 0, xScale);
                            //characterInfos[i].vertex_BL.uv2 = vertexData;
                            //characterInfos[i].vertex_TL.uv2 = vertexData;
                            //characterInfos[i].vertex_TR.uv2 = vertexData;
                            //characterInfos[i].vertex_BR.uv2 = vertexData;

                            float x0 = characterInfos[i].vertex_BL.uv2.x;
                            float y0 = characterInfos[i].vertex_BL.uv2.y;
                            float x1 = characterInfos[i].vertex_TR.uv2.x;
                            float y1 = characterInfos[i].vertex_TR.uv2.y;

                            float dx = Mathf.Floor(x0);
                            float dy = Mathf.Floor(y0);

                            x0 = x0 - dx;
                            x1 = x1 - dx;
                            y0 = y0 - dy;
                            y1 = y1 - dy;

                            // Optimization to avoid having a vector2 returned from the Pack UV function.
                            characterInfos[i].vertex_BL.uv2.x = PackUV(x0, y0); characterInfos[i].vertex_BL.uv2.y = xScale;
                            characterInfos[i].vertex_TL.uv2.x = PackUV(x0, y1); characterInfos[i].vertex_TL.uv2.y = xScale;
                            characterInfos[i].vertex_TR.uv2.x = PackUV(x1, y1); characterInfos[i].vertex_TR.uv2.y = xScale;
                            characterInfos[i].vertex_BR.uv2.x = PackUV(x1, y0); characterInfos[i].vertex_BR.uv2.y = xScale;
                            #if PROFILE_ON
                                Profiler.EndSample();
                            #endif
                            #endregion
                            break;
                        
                        // SPRITES
                        case TMP_TextElementType.Sprite:
                            // Nothing right now
                            break;
                    }

                    // Handle maxVisibleCharacters, maxVisibleLines and Overflow Page Mode.
                    #region Handle maxVisibleCharacters / maxVisibleLines / Page Mode
                    #if PROFILE_ON
                    Profiler.BeginSample("Process MaxVisible Characters & Lines");
                    #endif
                    if (i < m_maxVisibleCharacters && currentLine < m_maxVisibleLines && m_overflowMode != TextOverflowModes.Page)
                    {
                        characterInfos[i].vertex_BL.position += offset;
                        characterInfos[i].vertex_TL.position += offset;
                        characterInfos[i].vertex_TR.position += offset;
                        characterInfos[i].vertex_BR.position += offset;
                    }
                    else if (i < m_maxVisibleCharacters && currentLine < m_maxVisibleLines && m_overflowMode == TextOverflowModes.Page && characterInfos[i].pageNumber == pageToDisplay)
                    {
                        characterInfos[i].vertex_BL.position += offset;
                        characterInfos[i].vertex_TL.position += offset;
                        characterInfos[i].vertex_TR.position += offset;
                        characterInfos[i].vertex_BR.position += offset;
                    }
                    else
                    {
                        characterInfos[i].vertex_BL.position = Vector3.zero;
                        characterInfos[i].vertex_TL.position = Vector3.zero;
                        characterInfos[i].vertex_TR.position = Vector3.zero;
                        characterInfos[i].vertex_BR.position = Vector3.zero;
                    }
                    #if PROFILE_ON
                    Profiler.EndSample();
                    #endif
                    #endregion


                    // Fill Vertex Buffers for the various types of element
                    if (elementType == TMP_TextElementType.Character)
                    {
                        FillCharacterVertexBuffers(i, vert_index_X4);
                    }
                    else if (elementType == TMP_TextElementType.Sprite)
                    {
                        FillSpriteVertexBuffers(i, sprite_index_X4);
                    }
                }
                #endregion

                // Apply Alignment and Justification Offset
                m_textInfo.characterInfo[i].bottomLeft += offset;
                m_textInfo.characterInfo[i].topLeft += offset;
                m_textInfo.characterInfo[i].topRight += offset;
                m_textInfo.characterInfo[i].bottomRight += offset;

                m_textInfo.characterInfo[i].origin += offset.x;
                m_textInfo.characterInfo[i].xAdvance += offset.x;

                m_textInfo.characterInfo[i].ascender += offset.y;
                m_textInfo.characterInfo[i].descender += offset.y;
                m_textInfo.characterInfo[i].baseLine += offset.y;

                // Update MeshExtents
                if (isCharacterVisible)
                {
                    //m_meshExtents.min = new Vector2(Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[i].bottomLeft.x), Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[i].bottomLeft.y));
                    //m_meshExtents.max = new Vector2(Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[i].topRight.x), Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[i].topLeft.y));
                }

                // Need to recompute lineExtent to account for the offset from justification.
                #region Adjust lineExtents resulting from alignment offset
                #if PROFILE_ON
                    Profiler.BeginSample("Adjust LineExtents");
                #endif
                if (currentLine != lastLine || i == m_characterCount - 1)
                {
                    // Update the previous line's extents
                    if (currentLine != lastLine)
                    {
                        m_textInfo.lineInfo[lastLine].baseline += offset.y;
                        m_textInfo.lineInfo[lastLine].ascender += offset.y;
                        m_textInfo.lineInfo[lastLine].descender += offset.y;

                        m_textInfo.lineInfo[lastLine].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lastLine].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[lastLine].descender);
                        m_textInfo.lineInfo[lastLine].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lastLine].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[lastLine].ascender);
                    }

                    // Update the current line's extents
                    if (i == m_characterCount - 1)
                    {
                        m_textInfo.lineInfo[currentLine].baseline += offset.y;
                        m_textInfo.lineInfo[currentLine].ascender += offset.y;
                        m_textInfo.lineInfo[currentLine].descender += offset.y;

                        m_textInfo.lineInfo[currentLine].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[currentLine].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[currentLine].descender);
                        m_textInfo.lineInfo[currentLine].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[currentLine].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[currentLine].ascender);
                    }
                }
#if PROFILE_ON
                    Profiler.EndSample();
#endif
                #endregion


                // Track Word Count per line and for the object
                #region Track Word Count
                #if PROFILE_ON
                Profiler.BeginSample("Track Word Count");
                #endif
                if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == 0x2D || currentCharacter == 0xAD || currentCharacter == 0x2010 || currentCharacter == 0x2011)
                {
                    if (isStartOfWord == false)
                    {
                        isStartOfWord = true;
                        wordFirstChar = i;
                    }

                    // If last character is a word
                    if (isStartOfWord && i == m_characterCount - 1)
                    {
                        int size = m_textInfo.wordInfo.Length;
                        int index = m_textInfo.wordCount;

                        if (m_textInfo.wordCount + 1 > size)
                            TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                        wordLastChar = i;

                        m_textInfo.wordInfo[index].firstCharacterIndex = wordFirstChar;
                        m_textInfo.wordInfo[index].lastCharacterIndex = wordLastChar;
                        m_textInfo.wordInfo[index].characterCount = wordLastChar - wordFirstChar + 1;
                        m_textInfo.wordInfo[index].textComponent = this;

                        wordCount += 1;
                        m_textInfo.wordCount += 1;
                        m_textInfo.lineInfo[currentLine].wordCount += 1;
                    }
                }
                else if (isStartOfWord || i == 0 && (!char.IsPunctuation(currentCharacter) || char.IsWhiteSpace(currentCharacter) || i == m_characterCount - 1))
                {
                    if (i > 0 && i < characterInfos.Length - 1 && i < m_characterCount && (currentCharacter == 39 || currentCharacter == 8217) && char.IsLetterOrDigit(characterInfos[i - 1].character) && char.IsLetterOrDigit(characterInfos[i + 1].character))
                    {

                    }
                    else
                    {
                        wordLastChar = i == m_characterCount - 1 && char.IsLetterOrDigit(currentCharacter) ? i : i - 1;
                        isStartOfWord = false;

                        int size = m_textInfo.wordInfo.Length;
                        int index = m_textInfo.wordCount;

                        if (m_textInfo.wordCount + 1 > size)
                            TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                        m_textInfo.wordInfo[index].firstCharacterIndex = wordFirstChar;
                        m_textInfo.wordInfo[index].lastCharacterIndex = wordLastChar;
                        m_textInfo.wordInfo[index].characterCount = wordLastChar - wordFirstChar + 1;
                        m_textInfo.wordInfo[index].textComponent = this;

                        wordCount += 1;
                        m_textInfo.wordCount += 1;
                        m_textInfo.lineInfo[currentLine].wordCount += 1;
                    }
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion

                // Setup & Handle Underline
                #region Underline & Strikethrough
                #if PROFILE_ON
                Profiler.BeginSample("Process Underline & Strikethrough");
                #endif
                // NOTE: Need to figure out how underline will be handled with multiple fonts and which font will be used for the underline.
                bool isUnderline = (m_textInfo.characterInfo[i].style & FontStyles.Underline) == FontStyles.Underline;
                if (isUnderline)
                {
                    bool isUnderlineVisible = true;
                    int currentPage = m_textInfo.characterInfo[i].pageNumber;

                    if (i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && currentPage + 1 != m_pageToDisplay))
                        isUnderlineVisible = false;

                    // We only use the scale of visible characters.
                    if (!char.IsWhiteSpace(currentCharacter))
                    {
                        underlineMaxScale = Mathf.Max(underlineMaxScale, m_textInfo.characterInfo[i].scale);
                        underlineBaseLine = Mathf.Min(currentPage == lastPage ? underlineBaseLine : k_LargePositiveFloat, m_textInfo.characterInfo[i].baseLine + font.fontInfo.Underline * underlineMaxScale);
                        lastPage = currentPage; // Need to track pages to ensure we reset baseline for the new pages.
                    }

                    if (beginUnderline == false && isUnderlineVisible == true && i <= lineInfo.lastVisibleCharacterIndex && currentCharacter != 10 && currentCharacter != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(currentCharacter))
                        { }
                        else
                        {
                            beginUnderline = true;
                            underlineStartScale = m_textInfo.characterInfo[i].scale;
                            if (underlineMaxScale == 0) underlineMaxScale = underlineStartScale;
                            underline_start = new Vector3(m_textInfo.characterInfo[i].bottomLeft.x, underlineBaseLine, 0);
                            underlineColor = m_textInfo.characterInfo[i].color;
                        }
                    }

                    // End Underline if text only contains one character.
                    if (beginUnderline && m_characterCount == 1)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, xScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = k_LargePositiveFloat;
                    }
                    else if (beginUnderline && (i == lineInfo.lastCharacterIndex || i >= lineInfo.lastVisibleCharacterIndex))
                    {
                        // Terminate underline at previous visible character if space or carriage return.
                        if (char.IsWhiteSpace(currentCharacter))
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            underline_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
                        }
                        else
                        {   // End underline if last character of the line.
                            underline_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[i].scale;
                        }

                        beginUnderline = false;
                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, xScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = k_LargePositiveFloat;
                    }
                    else if (beginUnderline && !isUnderlineVisible)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, xScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = k_LargePositiveFloat;
                    }
                }
                else
                {
                    // End Underline
                    if (beginUnderline == true)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, xScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = k_LargePositiveFloat;
                    }
                }


                // Setup & Handle Strikethrough
                // NOTE: Need to figure out how underline will be handled with multiple fonts and which font will be used for the underline.
                bool isStrikethrough = (m_textInfo.characterInfo[i].style & FontStyles.Strikethrough) == FontStyles.Strikethrough;
                if (isStrikethrough)
                {
                    bool isStrikeThroughVisible = true;

                    if (i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && m_textInfo.characterInfo[i].pageNumber + 1 != m_pageToDisplay))
                        isStrikeThroughVisible = false;

                    if (beginStrikethrough == false && isStrikeThroughVisible && i <= lineInfo.lastVisibleCharacterIndex && currentCharacter != 10 && currentCharacter != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(currentCharacter))
                        { }
                        else
                        {
                            beginStrikethrough = true;
                            strikethroughPointSize = m_textInfo.characterInfo[i].pointSize;
                            strikethroughScale = m_textInfo.characterInfo[i].scale;
                            strikethrough_start = new Vector3(m_textInfo.characterInfo[i].bottomLeft.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2.75f * strikethroughScale, 0);
                            strikethroughColor = m_textInfo.characterInfo[i].color;
                            strikethroughBaseline = m_textInfo.characterInfo[i].baseLine;
                            //Debug.Log("Char [" + currentCharacter + "] Start Strikethrough POS: " + strikethrough_start);
                        }
                    }

                    // End Strikethrough if text only contains one character.
                    if (beginStrikethrough && m_characterCount == 1)
                    {
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i == lineInfo.lastCharacterIndex)
                    {
                        // Terminate Strikethrough at previous visible character if space or carriage return.
                        if (char.IsWhiteSpace(currentCharacter))
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        }
                        else
                        {
                            // Terminate Strikethrough at last character of line.
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        }

                        beginStrikethrough = false;
                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i < m_characterCount && (m_textInfo.characterInfo[i + 1].pointSize != strikethroughPointSize || !TMP_Math.Approximately(m_textInfo.characterInfo[i + 1].baseLine + offset.y, strikethroughBaseline)))
                    {
                        // Terminate Strikethrough if scale changes.
                        beginStrikethrough = false;

                        int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                        if (i > lastVisibleCharacterIndex)
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        else
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                        //Debug.Log("Char [" + currentCharacter + "] at Index: " + i + "  End Strikethrough POS: " + strikethrough_end + "  Baseline: " + m_textInfo.characterInfo[i].baseLine.ToString("f3"));
                    }
                    else if (beginStrikethrough && !isStrikeThroughVisible)
                    {
                        // Terminate Strikethrough if character is not visible.
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                }
                else
                {
                    // End Underline
                    if (beginStrikethrough == true)
                    {
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * m_fontScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                }
                #if PROFILE_ON
                Profiler.EndSample();
                #endif
                #endregion

                lastLine = currentLine;
            }
            #endregion

            // METRICS ABOUT THE TEXT OBJECT
            m_textInfo.characterCount = (short)m_characterCount;
            m_textInfo.spriteCount = m_spriteCount;
            m_textInfo.lineCount = (short)lineCount;
            m_textInfo.wordCount = wordCount != 0 && m_characterCount > 0 ? (short)wordCount : (short)1;
            m_textInfo.pageCount = m_pageNumber + 1;

            #if PROFILE_PHASES_ON
                Profiler.EndSample();
            #endif


            // *** UPLOAD MESH DATA ***
            #if PROFILE_PHASES_ON
                Profiler.BeginSample("TMP Generate Text - Phase III");
            #endif
            if (m_renderMode == TextRenderFlags.Render)
            {
                // Clear unused vertices
                //m_textInfo.meshInfo[0].ClearUnusedVertices();

                // Upload Mesh Data
                m_mesh.MarkDynamic();
                m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
                m_mesh.uv = m_textInfo.meshInfo[0].uvs0;
                m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
                //m_mesh.uv4 = m_textInfo.meshInfo[0].uvs4;
                m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;

                // Compute Bounds for the mesh. Manual computation is more efficient then using Mesh.recalcualteBounds.
                m_mesh.RecalculateBounds();
                //m_mesh.bounds = new Bounds(new Vector3((m_meshExtents.max.x + m_meshExtents.min.x) / 2, (m_meshExtents.max.y + m_meshExtents.min.y) / 2, 0) + offset, new Vector3(m_meshExtents.max.x - m_meshExtents.min.x, m_meshExtents.max.y - m_meshExtents.min.y, 0));

                m_canvasRenderer.SetMesh(m_mesh);

                for (int i = 1; i < m_textInfo.materialCount; i++)
                {
                    // Clear unused vertices
                    m_textInfo.meshInfo[i].ClearUnusedVertices();

                    if (m_subTextObjects[i] == null) continue;

                    m_subTextObjects[i].mesh.MarkDynamic();
                    m_subTextObjects[i].mesh.vertices = m_textInfo.meshInfo[i].vertices;
                    m_subTextObjects[i].mesh.uv = m_textInfo.meshInfo[i].uvs0;
                    m_subTextObjects[i].mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
                    //m_subTextObjects[i].mesh.uv3 = m_textInfo.meshInfo[i].uvs4;
                    m_subTextObjects[i].mesh.colors32 = m_textInfo.meshInfo[i].colors32;

                    m_subTextObjects[i].mesh.RecalculateBounds();

                    m_subTextObjects[i].canvasRenderer.SetMesh(m_subTextObjects[i].mesh);
                }
            }

            // Event indicating the text has been regenerated.
            TMPro_EventManager.ON_TEXT_CHANGED(this);

            #if PROFILE_PHASES_ON
                Profiler.EndSample();
            #endif

            //Debug.Log("Done Rendering Text.");
        }


        /// <summary>
        /// Method to return the local corners of the Text Container or RectTransform.
        /// </summary>
        /// <returns></returns>
        protected override Vector3[] GetTextContainerLocalCorners()
        {
            if (m_rectTransform == null) m_rectTransform = this.rectTransform;

            m_rectTransform.GetLocalCorners(m_RectTransformCorners);

            return m_RectTransformCorners;
        }


        /// <summary>
        /// Method to clear the mesh.
        /// </summary>
        void ClearMesh()
        {
            m_canvasRenderer.SetMesh(null);

            for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
                m_subTextObjects[i].canvasRenderer.SetMesh(null);
        }


        /// <summary>
        /// Method to Enable or Disable child SubMesh objects.
        /// </summary>
        /// <param name="state"></param>
        protected override void SetActiveSubMeshes(bool state)
        {
            for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
            {
                if (m_subTextObjects[i].enabled != state)
                    m_subTextObjects[i].enabled = state;
            }
        }


        /// <summary>
        ///  Method returning the compound bounds of the text object and child sub objects.
        /// </summary>
        /// <returns></returns>
        protected override Bounds GetCompoundBounds()
        {
            Bounds mainBounds = m_mesh.bounds;
            Vector2 min = mainBounds.min;
            Vector2 max = mainBounds.max;

            for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
            {
                Bounds subBounds = m_subTextObjects[i].mesh.bounds;
                min.x = min.x < subBounds.min.x ? min.x : subBounds.min.x;
                min.y = min.y < subBounds.min.y ? min.y : subBounds.min.y;

                max.x = max.x > subBounds.max.x ? max.x : subBounds.max.x;
                max.y = max.y > subBounds.max.y ? max.y : subBounds.max.y;
            }

            Vector2 center = (min + max) / 2;
            Vector2 size = max - min;
            return new Bounds(center, size);
        }


        //public override void UpdateGeometry()
        //{

        //}


        /// <summary>
        /// Method to Update Scale in UV2
        /// </summary>
        void UpdateSDFScale(float lossyScale)
        {
            //Debug.Log("Updating SDF Scale.");

            lossyScale = lossyScale == 0 ? 1 : lossyScale;

            float xScale = 0;
            float canvasScaleFactor = m_canvas.scaleFactor;

            if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                xScale = lossyScale / canvasScaleFactor;
            else if (m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                xScale = m_canvas.worldCamera != null ? lossyScale : 1;
            else
                xScale = lossyScale;

            // Iterate through each of the characters.
            for (int i = 0; i < m_textInfo.characterCount; i++)
            {
                // Only update scale for visible characters.
                if (m_textInfo.characterInfo[i].isVisible && m_textInfo.characterInfo[i].elementType == TMP_TextElementType.Character)
                {
                    float scale = xScale * m_textInfo.characterInfo[i].scale * (1 - m_charWidthAdjDelta);
                    if (!m_textInfo.characterInfo[i].isUsingAlternateTypeface && (m_textInfo.characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold) scale *= -1;

                    int index = m_textInfo.characterInfo[i].materialReferenceIndex;
                    int vertexIndex = m_textInfo.characterInfo[i].vertexIndex;

                    m_textInfo.meshInfo[index].uvs2[vertexIndex + 0].y = scale;
                    m_textInfo.meshInfo[index].uvs2[vertexIndex + 1].y = scale;
                    m_textInfo.meshInfo[index].uvs2[vertexIndex + 2].y = scale;
                    m_textInfo.meshInfo[index].uvs2[vertexIndex + 3].y = scale;
                }
            }

            // Push the updated uv2 scale information to the meshes.
            for (int i = 0; i < m_textInfo.materialCount; i++)
            {
                if (i == 0)
                {
                    m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
                    m_canvasRenderer.SetMesh(m_mesh);
                }
                else
                {
                    m_subTextObjects[i].mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
                    m_subTextObjects[i].canvasRenderer.SetMesh(m_subTextObjects[i].mesh);
                }
            }
        }


        // Function to offset vertices position to account for line spacing changes.
        protected override void AdjustLineOffset(int startIndex, int endIndex, float offset)
        {
            Vector3 vertexOffset = new Vector3(0, offset, 0);

            for (int i = startIndex; i <= endIndex; i++)
            {
                m_textInfo.characterInfo[i].bottomLeft -= vertexOffset;
                m_textInfo.characterInfo[i].topLeft -= vertexOffset;
                m_textInfo.characterInfo[i].topRight -= vertexOffset;
                m_textInfo.characterInfo[i].bottomRight -= vertexOffset;

                m_textInfo.characterInfo[i].ascender -= vertexOffset.y;
                m_textInfo.characterInfo[i].baseLine -= vertexOffset.y;
                m_textInfo.characterInfo[i].descender -= vertexOffset.y;

                if (m_textInfo.characterInfo[i].isVisible)
                {
                    m_textInfo.characterInfo[i].vertex_BL.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_TL.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_TR.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_BR.position -= vertexOffset;
                }
            }
        }

    }
}