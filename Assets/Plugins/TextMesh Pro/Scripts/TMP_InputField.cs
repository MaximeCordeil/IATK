// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace TMPro
{
    /// <summary>
    /// Editable text input field.
    /// </summary>
    [AddComponentMenu("UI/TextMeshPro - Input Field", 11)]
    public class TMP_InputField : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICanvasElement
    {

        // Setting the content type acts as a shortcut for setting a combination of InputType, CharacterValidation, LineType, and TouchScreenKeyboardType
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation
        {
            None,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            EmailAddress
        }

        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class OnChangeEvent : UnityEvent<string> { }

        protected TouchScreenKeyboard m_Keyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

        #region Exposed properties
        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary>

        [SerializeField]
        protected RectTransform m_TextViewport;

        //Vector3[] m_ViewportCorners = new Vector3[4];

        [SerializeField]
        protected TMP_Text m_TextComponent;

        protected RectTransform m_TextComponentRectTransform;

        [SerializeField]
        protected Graphic m_Placeholder;

        [SerializeField]
        private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [SerializeField]
        private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [SerializeField]
        private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [SerializeField]
        private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input.
        /// </summary>
        [SerializeField]
        private bool m_HideMobileInput = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [SerializeField]
        private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [SerializeField]
        private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnEndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnSubmit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field focus is lost.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnFocusLost = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [SerializeField]
        private OnValidateInput m_OnValidateInput;

        [SerializeField]
        private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        [SerializeField]
        private bool m_CustomCaretColor = false;

        [SerializeField]
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>

        [SerializeField]
        protected string m_Text = string.Empty;

        [SerializeField]
        [Range(0f, 4f)]
        private float m_CaretBlinkRate = 0.85f;

        [SerializeField]
        [Range(1, 5)]
        private int m_CaretWidth = 1;

        [SerializeField]
        private bool m_ReadOnly = false;

        [SerializeField]
        private bool m_RichText = true;

        #endregion

        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        //private TextGenerator m_InputTextCache;
        private CanvasRenderer m_CachedInputRenderer;
        //private bool m_PreventFontCallback = false;

        [NonSerialized]
        protected Mesh m_Mesh;
        private bool m_AllowInput = false;
        private bool m_HasLostFocus = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        protected int m_DrawStart = 0;
        protected int m_DrawEnd = 0;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;

        private bool m_isLastKeyBackspace = false;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";


        protected TMP_InputField()
        { }

        protected Mesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        //protected TextGenerator cachedInputTextGenerator
        //{
        //    get
        //    {
        //        if (m_InputTextCache == null)
        //            m_InputTextCache = new TextGenerator();

        //        return m_InputTextCache;
        //    }
        //}

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>
        public bool shouldHideMobileInput
        {
            set
            {
                SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
            }
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.TizenPlayer:
#if UNITY_5_3 || UNITY_5_4
                    case RuntimePlatform.tvOS:
#endif
#if !UNITY_5_4
                    case RuntimePlatform.BlackBerryPlayer:
#endif
                        return m_HideMobileInput;
                }

                return true;
            }
        }


        /// <summary>
        /// Input field's current text value.
        /// </summary>

        public string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (this.text == value)
                    return;

                m_Text = value;

                
                //if (m_LineType == LineType.SingleLine)
                //    m_Text = m_Text.Replace("\n", "").Replace("\t", "");

                //// If we have an input validator, validate the input and apply the character limit at the same time.
                //if (onValidateInput != null || characterValidation != CharacterValidation.None)
                //{
                //    m_Text = "";
                //    OnValidateInput validatorMethod = onValidateInput ?? Validate;
                //    m_CaretPosition = m_CaretSelectPosition = value.Length;
                //    int charactersToCheck = characterLimit > 0 ? Math.Min(characterLimit - 1, value.Length) : value.Length;
                //    for (int i = 0; i < charactersToCheck; ++i)
                //    {
                //        char c = validatorMethod(m_Text, m_Text.Length, value[i]);
                //        if (c != 0)
                //            m_Text += c;
                //    }
                //}
                //else
                //{
                //    m_Text = characterLimit > 0 && value.Length > characterLimit ? value.Substring(0, characterLimit) : value;
                //}

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    SendOnValueChangedAndUpdateLabel();
                    return;
                }
#endif

                if (m_Keyboard != null)
                    m_Keyboard.text = m_Text;

                if (m_StringPosition > m_Text.Length)
                    m_StringPosition = m_StringSelectPosition = m_Text.Length;

                SendOnValueChangedAndUpdateLabel();
            }
        }

        public bool isFocused
        {
            get { return m_AllowInput; }
        }

        public float caretBlinkRate
        {
            get { return m_CaretBlinkRate; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_CaretBlinkRate, value))
                {
                    if (m_AllowInput)
                        SetCaretActive();
                }
            }
        }

        public int caretWidth { get { return m_CaretWidth; } set { if (SetPropertyUtility.SetStruct(ref m_CaretWidth, value)) MarkGeometryAsDirty(); } }

        public RectTransform textViewport { get { return m_TextViewport; } set { SetPropertyUtility.SetClass(ref m_TextViewport, value); } }

        public TMP_Text textComponent { get { return m_TextComponent; } set { SetPropertyUtility.SetClass(ref m_TextComponent, value); } }

        public Graphic placeholder { get { return m_Placeholder; } set { SetPropertyUtility.SetClass(ref m_Placeholder, value); } }

        public Color caretColor { get { return customCaretColor ? m_CaretColor : textComponent.color; } set { if (SetPropertyUtility.SetColor(ref m_CaretColor, value)) MarkGeometryAsDirty(); } }

        public bool customCaretColor { get { return m_CustomCaretColor; } set { if (m_CustomCaretColor != value) { m_CustomCaretColor = value; MarkGeometryAsDirty(); } } }

        public Color selectionColor { get { return m_SelectionColor; } set { if (SetPropertyUtility.SetColor(ref m_SelectionColor, value)) MarkGeometryAsDirty(); } }

        public SubmitEvent onEndEdit { get { return m_OnEndEdit; } set { SetPropertyUtility.SetClass(ref m_OnEndEdit, value); } }

        public SubmitEvent onSubmit { get { return m_OnSubmit; } set { SetPropertyUtility.SetClass(ref m_OnSubmit, value); } }

        public SubmitEvent onFocusLost { get { return m_OnFocusLost; } set { SetPropertyUtility.SetClass(ref m_OnFocusLost, value); } }

        //[Obsolete("onValueChange has been renamed to onValueChanged")]
        //public OnChangeEvent onValueChange { get { return onValueChanged; } set { onValueChanged = value; } }

        public OnChangeEvent onValueChanged { get { return m_OnValueChanged; } set { SetPropertyUtility.SetClass(ref m_OnValueChanged, value); } }

        public OnValidateInput onValidateInput { get { return m_OnValidateInput; } set { SetPropertyUtility.SetClass(ref m_OnValidateInput, value); } }

        public int characterLimit { get { return m_CharacterLimit; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterLimit, Math.Max(0, value))) UpdateLabel(); } }

        // Content Type related

        public ContentType contentType { get { return m_ContentType; } set { if (SetPropertyUtility.SetStruct(ref m_ContentType, value)) EnforceContentType(); } }

        public LineType lineType { get { return m_LineType; } set { if (SetPropertyUtility.SetStruct(ref m_LineType, value)) SetTextComponentWrapMode(); SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected); } }

        public InputType inputType { get { return m_InputType; } set { if (SetPropertyUtility.SetStruct(ref m_InputType, value)) SetToCustom(); } }

        public TouchScreenKeyboardType keyboardType { get { return m_KeyboardType; } set { if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value)) SetToCustom(); } }

        public CharacterValidation characterValidation { get { return m_CharacterValidation; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value)) SetToCustom(); } }

        public bool readOnly { get { return m_ReadOnly; } set { m_ReadOnly = value; } }

        public bool richText { get { return m_RichText; } set { m_RichText = value; SetTextComponentRichTextMode(); } }

        // Derived property
        public bool multiLine { get { return m_LineType == LineType.MultiLineNewline || lineType == LineType.MultiLineSubmit; } }
        // Not shown in Inspector.
        public char asteriskChar { get { return m_AsteriskChar; } set { if (SetPropertyUtility.SetStruct(ref m_AsteriskChar, value)) UpdateLabel(); } }
        public bool wasCanceled { get { return m_WasCanceled; } }

        protected void ClampPos(ref int pos)
        {
            if (pos < 0) pos = 0;
            else if (pos > text.Length) pos = text.Length;
        }

        /// <summary>
        /// Current position of the cursor.
        /// Getters are public Setters are protected
        /// </summary>

        protected int caretPositionInternal { get { return m_CaretPosition + Input.compositionString.Length; } set { m_CaretPosition = value; ClampPos(ref m_CaretPosition); } }
        protected int stringPositionInternal { get { return m_StringPosition + Input.compositionString.Length; } set { m_StringPosition = value; ClampPos(ref m_StringPosition); } }

        protected int caretSelectPositionInternal { get { return m_CaretSelectPosition + Input.compositionString.Length; } set { m_CaretSelectPosition = value; ClampPos(ref m_CaretSelectPosition); } }
        protected int stringSelectPositionInternal { get { return m_StringSelectPosition + Input.compositionString.Length; } set { m_StringSelectPosition = value; ClampPos(ref m_StringSelectPosition); } }

        private bool hasSelection { get { return stringPositionInternal != stringSelectPositionInternal; } }
        private bool isCaretInsideTag;


        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>

        public int caretPosition
        {
            get { return m_StringSelectPosition + Input.compositionString.Length; }
            set { selectionAnchorPosition = value; selectionFocusPosition = value; }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If Input.compositionString is 0 set the fixed position
        /// </summary>

        public int selectionAnchorPosition
        {
            get
            {
                m_StringPosition = GetStringIndexFromCaretPosition(m_CaretPosition);

                return m_StringPosition + Input.compositionString.Length;
            }
            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                // TODO: This should set m_StringPosition and be clamped.
                m_CaretPosition = value;
                ClampPos(ref m_CaretPosition); // ClampPos(ref m_StringPosition);
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If Input.compositionString is 0 set the variable position
        /// </summary>

        public int selectionFocusPosition
        {
            get
            {
                m_StringSelectPosition = GetStringIndexFromCaretPosition(m_CaretSelectPosition);

                return m_StringSelectPosition + Input.compositionString.Length;
            }
            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                // TODO: This should set m_StringSelectPosition and be clamped.
                m_CaretSelectPosition = value;
                ClampPos(ref m_CaretSelectPosition);
            }
        }

#if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate()
        {
            base.OnValidate();
            EnforceContentType();

            m_CharacterLimit = Math.Max(0, m_CharacterLimit);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            SetTextComponentRichTextMode();

            UpdateLabel();
            if (m_AllowInput)
                SetCaretActive();
        }

#endif // if UNITY_EDITOR

        protected override void OnEnable()
        {
            //Debug.Log("*** OnEnable() *** - " + this.name);

            base.OnEnable();

            if (m_Text == null)
                m_Text = string.Empty;

            m_DrawStart = 0;
            m_DrawEnd = m_Text.Length;

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

            if (m_TextComponent != null)
            {
                m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);

                UpdateLabel();
            }
        }

        protected override void OnDisable()
        {
            // the coroutine will be terminated, so this will ensure it restarts when we are next activated
            m_BlinkCoroutine = null;

            DeactivateInputField();
            if (m_TextComponent != null)
            {
                m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
            }
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            // Clear needs to be called otherwise sync never happens as the object is disabled.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.Clear();

            if (m_Mesh != null)
                DestroyImmediate(m_Mesh);
            m_Mesh = null;

            base.OnDisable();
        }


        IEnumerator CaretBlink()
        {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while (isFocused && m_CaretBlinkRate > 0)
            {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (m_CaretVisible != blinkState)
                {
                    m_CaretVisible = blinkState;
                    if (!hasSelection)
                        MarkGeometryAsDirty();
                }

                // Then wait again.
                yield return null;
            }
            m_BlinkCoroutine = null;
        }

        void SetCaretVisible()
        {
            if (!m_AllowInput)
                return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f)
            {
                if (m_BlinkCoroutine == null)
                    m_BlinkCoroutine = StartCoroutine(CaretBlink());
            }
            else
            {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus()
        {
            SelectAll();
        }

        protected void SelectAll()
        {
            stringPositionInternal = text.Length;
            stringSelectPositionInternal = 0;
        }

        public void MoveTextEnd(bool shift)
        {
            int position = text.Length;

            if (shift)
            {
                stringSelectPositionInternal = position;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;
            }
            UpdateLabel();
        }

        public void MoveTextStart(bool shift)
        {
            int position = 0;

            if (shift)
            {
                stringSelectPositionInternal = position;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;
            }

            UpdateLabel();
        }

        static string clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private bool InPlaceEditing()
        {
            return !TouchScreenKeyboard.isSupported;
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        protected virtual void LateUpdate()
        {
            // Only activate if we are not already activated.
            if (m_ShouldActivateNextUpdate)
            {
                if (!isFocused)
                {
                    ActivateInputFieldInternal();
                    m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                m_ShouldActivateNextUpdate = false;
            }

            if (InPlaceEditing() || !isFocused)
                return;

            AssignPositioningIfNeeded();

            if (m_Keyboard == null || !m_Keyboard.active)
            {
                if (m_Keyboard != null)
                {
                    if (!m_ReadOnly)
                        text = m_Keyboard.text;

                    if (m_Keyboard.wasCanceled)
                        m_WasCanceled = true;
                }

                OnDeselect(null);
                return;
            }

            string val = m_Keyboard.text;

            if (m_Text != val)
            {
                if (m_ReadOnly)
                {
                    m_Keyboard.text = m_Text;
                }
                else
                {
                    m_Text = "";

                    for (int i = 0; i < val.Length; ++i)
                    {
                        char c = val[i];

                        if (c == '\r' || (int)c == 3)
                            c = '\n';

                        if (onValidateInput != null)
                            c = onValidateInput(m_Text, m_Text.Length, c);
                        else if (characterValidation != CharacterValidation.None)
                            c = Validate(m_Text, m_Text.Length, c);

                        if (lineType == LineType.MultiLineSubmit && c == '\n')
                        {
                            m_Keyboard.text = m_Text;

                            OnDeselect(null);
                            return;
                        }

                        if (c != 0)
                            m_Text += c;
                    }

                    if (characterLimit > 0 && m_Text.Length > characterLimit)
                        m_Text = m_Text.Substring(0, characterLimit);
                    stringPositionInternal = stringSelectPositionInternal = m_Text.Length;

                    // Set keyboard text before updating label, as we might have changed it with validation
                    // and update label will take the old value from keyboard if we don't change it here
                    if (m_Text != val)
                        m_Keyboard.text = m_Text;

                    SendOnValueChangedAndUpdateLabel();
                }
            }


            if (m_Keyboard.done)
            {
                if (m_Keyboard.wasCanceled)
                    m_WasCanceled = true;

                OnDeselect(null);
            }
        }

        //public Vector2 ScreenToLocal(Vector2 screen)
        //{
        //    var theCanvas = m_TextComponent.canvas;
        //    if (theCanvas == null)
        //        return screen;

        //    Vector3 pos = Vector3.zero;
        //    if (theCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        //    {
        //        pos = m_TextComponent.transform.InverseTransformPoint(screen);
        //    }
        //    else if (theCanvas.worldCamera != null)
        //    {
        //        Ray mouseRay = theCanvas.worldCamera.ScreenPointToRay(screen);
        //        float dist;
        //        Plane plane = new Plane(m_TextComponent.transform.forward, m_TextComponent.transform.position);
        //        plane.Raycast(mouseRay, out dist);
        //        pos = m_TextComponent.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
        //    }
        //    return new Vector2(pos.x, pos.y);
        //}

        //private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator)
        //{
        //    if (!multiLine)
        //        return 0;

        //    // transform y to local scale
        //    float y = pos.y * m_TextComponent.pixelsPerUnit;
        //    float lastBottomY = 0.0f;

        //    for (int i = 0; i < generator.lineCount; ++i)
        //    {
        //        float topY = generator.lines[i].topY;
        //        float bottomY = topY - generator.lines[i].height;

        //        // pos is somewhere in the leading above this line
        //        if (y > topY)
        //        {
        //            // determine which line we're closer to
        //            float leading = topY - lastBottomY;
        //            if (y > topY - 0.5f * leading)
        //                return i - 1;
        //            else
        //                return i;
        //        }

        //        if (y > bottomY)
        //            return i;

        //        lastBottomY = bottomY;
        //    }

        //    // Position is after last line.
        //    return generator.lineCount;
        //}

        /// <summary>
        /// Given an input position in local space on the Text return the index for the selection cursor at this position.
        /// </summary>

        protected int GetCharacterIndexFromPosition(Vector2 pos)
        {
            return 0;
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() &&
                   IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   m_TextComponent != null &&
                   m_Keyboard == null;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            CaretPosition insertionSide;
            int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

            if (insertionSide == CaretPosition.Left)
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
            else if (insertionSide == CaretPosition.Right)
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;

            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            MarkGeometryAsDirty();

            m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textViewport, eventData.position, eventData.pressEventCamera);
            if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
                m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

            eventData.Use();
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
        {
            while (m_UpdateDrag && m_DragPositionOutOfBounds)
            {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textViewport, eventData.position, eventData.pressEventCamera, out localMousePos);

                Rect rect = textViewport.rect;

                if (multiLine)
                {
                    if (localMousePos.y > rect.yMax)
                        MoveUp(true, true);
                    else if (localMousePos.y < rect.yMin)
                        MoveDown(true, true);
                }
                else
                {
                    if (localMousePos.x < rect.xMin)
                        MoveLeft(true, false);
                    else if (localMousePos.x > rect.xMax)
                        MoveRight(true, false);
                }

                UpdateLabel();

                float delay = multiLine ? kVScrollSpeed : kHScrollSpeed;
                yield return new WaitForSeconds(delay);
                //yield return new WaitForSecondsRealtime(delay); // Unity 5.4
            }
            m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (!InPlaceEditing())
            {
                if (m_Keyboard == null || !m_Keyboard.active)
                {
                    OnSelect(eventData);
                    return;
                }
            }

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore)
            {
                CaretPosition insertionSide;
                int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

                if (insertionSide == CaretPosition.Left)
                    stringPositionInternal = stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
                else if (insertionSide == CaretPosition.Right)
                    stringPositionInternal = stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;

                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);

            }
            UpdateLabel();
            eventData.Use();
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
#if UNITY_5_4
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
#else
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);// || rp == RuntimePlatform.OSXWebPlayer);
#endif
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    {
                        Backspace();
                        return EditState.Continue;
                    }

                case KeyCode.Delete:
                    {
                        ForwardSpace();
                        return EditState.Continue;
                    }

                case KeyCode.Home:
                    {
                        MoveTextStart(shift);
                        return EditState.Continue;
                    }

                case KeyCode.End:
                    {
                        MoveTextEnd(shift);
                        return EditState.Continue;
                    }

                // Select All
                case KeyCode.A:
                    {
                        if (ctrlOnly)
                        {
                            SelectAll();
                            return EditState.Continue;
                        }
                        break;
                    }

                // Copy
                case KeyCode.C:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            return EditState.Continue;
                        }
                        break;
                    }

                // Paste
                case KeyCode.V:
                    {
                        if (ctrlOnly)
                        {
                            Append(clipboard);
                            return EditState.Continue;
                        }
                        break;
                    }

                // Cut
                case KeyCode.X:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            Delete();
                            SendOnValueChangedAndUpdateLabel();
                            return EditState.Continue;
                        }
                        break;
                    }

                case KeyCode.LeftArrow:
                    {
                        MoveLeft(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.RightArrow:
                    {
                        MoveRight(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.UpArrow:
                    {
                        MoveUp(shift);
                        return EditState.Continue;
                    }

                case KeyCode.DownArrow:
                    {
                        MoveDown(shift);
                        return EditState.Continue;
                    }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    {
                        if (lineType != LineType.MultiLineNewline)
                        {
                            return EditState.Finish;
                        }
                        break;
                    }

                case KeyCode.Escape:
                    {
                        m_WasCanceled = true;
                        return EditState.Finish;
                    }
            }

            char c = evt.character;
            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (!multiLine && (c == '\t' || c == '\r' || c == 10))
                return EditState.Continue;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            if (IsValidChar(c))
            {
                Append(c);
            }

            if (c == 0)
            {
                if (Input.compositionString.Length > 0)
                {
                    UpdateLabel();
                }
            }
            return EditState.Continue;
        }

        private bool IsValidChar(char c)
        {
            // Delete key on mac
            if ((int)c == 127)
                return false;
            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            return m_TextComponent.font.HasCharacter(c, true);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            KeyPressed(e);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    consumedEvent = true;
                    var shouldContinue = KeyPressed(m_ProcessingEvent);
                    if (shouldContinue == EditState.Finish)
                    {
                        DeactivateInputField();
                        break;
                    }
                }

                switch (m_ProcessingEvent.type)
                {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (m_ProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            if (consumedEvent)
                UpdateLabel();

            eventData.Use();
        }

        private string GetSelectedString()
        {
            if (!hasSelection)
                return "";

            int startPos = stringPositionInternal;
            int endPos = stringSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos)
            {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            return text.Substring(startPos, endPos - startPos);
        }

        private int FindtNextWordBegin()
        {
            if (stringSelectPositionInternal + 1 >= text.Length)
                return text.Length;

            int spaceLoc = text.IndexOfAny(kSeparators, stringSelectPositionInternal + 1);

            if (spaceLoc == -1)
                spaceLoc = text.Length;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Max(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
                return;
            }

            int currentCaretPosition = caretSelectPositionInternal;
            int position;
            if (ctrl)
                position = FindtNextWordBegin();
            else
                position = stringSelectPositionInternal + 1;

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {
                stringSelectPositionInternal = stringPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            isCaretInsideTag = currentCaretPosition == caretSelectPositionInternal;
            Debug.Log("Caret is " + (isCaretInsideTag ? " [Inside Tag]" : " [Not Inside Tag]"));
        }

        private int FindtPrevWordBegin()
        {
            if (stringSelectPositionInternal - 2 < 0)
                return 0;

            int spaceLoc = text.LastIndexOfAny(kSeparators, stringSelectPositionInternal - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Min(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
                return;
            }

            int currentCaretPosition = caretSelectPositionInternal;
            int position;
            if (ctrl)
                position = FindtPrevWordBegin();
            else
                position = stringSelectPositionInternal - 1;

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {
                stringSelectPositionInternal = stringPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            isCaretInsideTag = currentCaretPosition == caretSelectPositionInternal;
            Debug.Log("Caret is " + (isCaretInsideTag ? " [Inside Tag]" : " [Not Inside Tag]"));
        }

        //private int DetermineCharacterLine(int charPos, TextGenerator generator)
        //{
        //    for (int i = 0; i < generator.lineCount - 1; ++i)
        //    {
        //        if (generator.lines[i + 1].startCharIdx > charPos)
        //            return i;
        //    }

        //    return generator.lineCount - 1;
        //}


        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine].firstCharacterIndex - 1;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine - 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = (originChar.origin - currentChar.origin) / (currentChar.xAdvance - currentChar.origin);

                if (d >= 0 && d <= 1)
                {
                    if (d < 0.5f)
                        return i;
                    else
                        return i + 1;
                }
            }
            return endCharIdx;
        }


        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                return text.Length;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            //// We are on the last line return last character
            if (originLine + 1 >= m_TextComponent.textInfo.lineCount)
                return goToLastChar ? m_TextComponent.textInfo.characterCount - 1 : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine + 1].lastCharacterIndex;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine + 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = (originChar.origin - currentChar.origin) / (currentChar.xAdvance - currentChar.origin);

                if (d >= 0 && d <= 1)
                {
                    if (d < 0.5f)
                        return i;
                    else
                        return i + 1;
                }
            }
            return endCharIdx;
        }


        private void MoveDown(bool shift)
        {
            MoveDown(shift, true);
        }

        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : text.Length;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
        }

        private void MoveUp(bool shift)
        {
            MoveUp(shift, true);
        }

        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
        }

        private void Delete()
        {
            if (m_ReadOnly)
                return;

            if (stringPositionInternal == stringSelectPositionInternal)
                return;

            if (stringPositionInternal < stringSelectPositionInternal)
            {
                m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);
                stringSelectPositionInternal = stringPositionInternal;
            }
            else
            {
                m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
                stringPositionInternal = stringSelectPositionInternal;
            }
        }

        private void ForwardSpace()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (stringPositionInternal < text.Length)
                {
                    m_Text = text.Remove(stringPositionInternal, 1);
                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        private void Backspace()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (stringPositionInternal > 0)
                {
                    m_Text = text.Remove(stringPositionInternal - 1, 1);
                    stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - 1;

                    m_isLastKeyBackspace = true;

                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        // Insert the character and update the label.
        private void Insert(char c)
        {
            if (m_ReadOnly)
                return;

            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            m_Text = text.Insert(m_StringPosition, replaceString);
            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;

            SendOnValueChanged();
        }

        private void SendOnValueChangedAndUpdateLabel()
        {
            SendOnValueChanged();
            UpdateLabel();
        }

        private void SendOnValueChanged()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(text);
        }

        /// <summary>
        /// Submit the input field's text.
        /// </summary>

        protected void SendOnSubmit()
        {
            if (onEndEdit != null)
                onEndEdit.Invoke(m_Text);
        }


        protected void SendOnFocusLost()
        {
            if (onFocusLost != null)
                onFocusLost.Invoke(m_Text);
        }

        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>

        protected virtual void Append(string input)
        {
            if (m_ReadOnly)
                return;

            if (!InPlaceEditing())
                return;

            for (int i = 0, imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
                {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input)
        {
            if (m_ReadOnly)
                return;

            if (!InPlaceEditing())
                return;

            // If we have an input validator, validate the input first
            if (onValidateInput != null)
                input = onValidateInput(text, stringPositionInternal, input);
            else if (characterValidation != CharacterValidation.None)
                input = Validate(text, stringPositionInternal, input);

            // If the input is invalid, skip it
            if (input == 0)
                return;

            // Append the character and update the label
            Insert(input);
        }

        /// <summary>
        /// Update the visual text Text.
        /// </summary>

        protected void UpdateLabel()
        {
            if (m_TextComponent != null && m_TextComponent.font != null)
            {
                // TextGenerator.Populate invokes a callback that's called for anything
                // that needs to be updated when the data for that font has changed.
                // This makes all Text components that use that font update their vertices.
                // In turn, this makes the InputField that's associated with that Text component
                // update its label by calling this UpdateLabel method.
                // This is a recursive call we want to prevent, since it makes the InputField
                // update based on font data that didn't yet finish executing, or alternatively
                // hang on infinite recursion, depending on whether the cached value is cached
                // before or after the calculation.
                //
                // This callback also occurs when assigning text to our Text component, i.e.,
                // m_TextComponent.text = processed;

                //m_PreventFontCallback = true;

                string fullText;
                if (Input.compositionString.Length > 0)
                    fullText = text.Substring(0, m_StringPosition) + Input.compositionString + text.Substring(m_StringPosition);
                else
                    fullText = text;

                string processed;
                if (inputType == InputType.Password)
                    processed = new string(asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty; // && !isFocused;

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.
                if (!m_AllowInput)
                {
                    m_DrawStart = 0;
                    m_DrawEnd = m_Text.Length;
                }

                if (!isEmpty)
                {
                //    // Determine what will actually fit into the given line
                //    Vector2 extents = m_TextComponent.rectTransform.rect.size;

                //    var settings = m_TextComponent.GetGenerationSettings(extents);
                //    settings.generateOutOfBounds = true;

                //    cachedInputTextGenerator.Populate(processed, settings);

                //    SetDrawRangeToContainCaretPosition(stringSelectPositionInternal - 1);

                //    processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

                    SetCaretVisible();
                }

                m_TextComponent.text = processed + "\u200B"; // Extra space is added for Caret tracking.
                MarkGeometryAsDirty();

                //m_PreventFontCallback = false;
            }
        }

        //private bool IsSelectionVisible()
        //{
        //    if (m_DrawStart > stringPositionInternal || m_DrawStart > stringSelectPositionInternal)
        //        return false;

        //    if (m_DrawEnd < stringPositionInternal || m_DrawEnd < stringSelectPositionInternal)
        //        return false;

        //    return true;
        //}


        private int GetCaretPositionFromStringIndex(int stringIndex)
        {
            int count = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition)
        {
            return m_TextComponent.textInfo.characterInfo[caretPosition].index;
        }



        public void ForceLabelUpdate()
        {
            UpdateLabel();
        }

        private void MarkGeometryAsDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null)
                return;
#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }


        private void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (!shouldHideMobileInput)
                return;

            if (m_CachedInputRenderer == null && m_TextComponent != null)
            {
                GameObject go = new GameObject(transform.name + " Input Caret");
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(m_TextComponent.transform.parent);
                go.transform.SetAsFirstSibling();
                go.layer = gameObject.layer;

                caretRectTrans = go.AddComponent<RectTransform>();
                m_CachedInputRenderer = go.AddComponent<CanvasRenderer>();
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

                // Needed as if any layout is present we want the caret to always be the same as the text area.
                go.AddComponent<LayoutElement>().ignoreLayout = true;

                AssignPositioningIfNeeded();
            }

            if (m_CachedInputRenderer == null)
                return;

            OnFillVBO(mesh);
            m_CachedInputRenderer.SetMesh(mesh);
        }


        private void AssignPositioningIfNeeded()
        {
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
            {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;

                // Get updated world corners of viewport.
                //m_TextViewport.GetLocalCorners(m_ViewportCorners);
            }
        }


        private void OnFillVBO(Mesh vbo)
        {
            using (var helper = new VertexHelper())
            {
                if (!isFocused)
                {
                    helper.FillMesh(vbo);
                    return;
                }


                // Adjust text position inside text viewport area if needed.
                //AssignPositioningIfNeeded();

                /*
                Rect inputRect = m_TextComponent.rectTransform.rect;
                Vector2 extents = inputRect.size;

                // get the text alignment anchor point for the text in local space
                Vector2 textAnchorPivot = m_TextComponent.rectTransform.pivot; // Text.GetTextAnchorPivot(m_TextComponent.alignment);
                Vector2 refPoint = Vector2.zero;

                refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
                refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

                // Adjust the anchor point in screen space
                Vector2 roundedRefPoint = m_TextComponent.PixelAdjustPoint(refPoint);

                // Determine fraction of pixel to offset text mesh.
                // This is the rounding in screen space, plus the fraction of a pixel the text anchor pivot is from the corner of the text mesh.
                Vector2 roundingOffset = roundedRefPoint - refPoint + Vector2.Scale(extents, textAnchorPivot);
                roundingOffset.x = roundingOffset.x - Mathf.Floor(0.5f + roundingOffset.x);
                roundingOffset.y = roundingOffset.y - Mathf.Floor(0.5f + roundingOffset.y);
                */

                if (!hasSelection)
                    GenerateCaret(helper, Vector2.zero);
                else
                    GenerateHightlight(helper, Vector2.zero);

                helper.FillMesh(vbo);
            }
        }

        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
        {
            if (!m_CaretVisible)
                return;

            if (m_CursorVerts == null)
            {
                CreateCursorVerts();
            }

            float width = m_CaretWidth;

            // Add check to only update the caret position when needed.
            //
            //

            int characterCount = m_TextComponent.textInfo.characterCount;
            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;

            // Get the position of the Caret based on position in the string.
            caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);

            if (caretPositionInternal == 0)
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[0];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else if (caretPositionInternal < characterCount)
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[caretPositionInternal];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[characterCount - 1];
                startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }

            //Debug.Log("String Char [" + m_Text[m_StringPosition] + "] at Index:" + m_StringPosition + "  Caret Char [" + currentCharacter.character + "] at Index:" + caretPositionInternal);

            // Adjust the position of the RectTransform based on the caret position in the viewport.
            AdjustRectTransformRelativeToViewport(startPosition, height, currentCharacter.isVisible);


            // Clamp Caret height
            float top = startPosition.y + height;
            float bottom = top - Mathf.Min(height, m_TextComponent.rectTransform.rect.height);

            m_CursorVerts[0].position = new Vector3(startPosition.x, bottom, 0.0f);
            m_CursorVerts[1].position = new Vector3(startPosition.x, top, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, top, 0.0f);
            m_CursorVerts[3].position = new Vector3(startPosition.x + width, bottom, 0.0f);

            // Set Vertex Color for the caret color.
            m_CursorVerts[0].color = caretColor;
            m_CursorVerts[1].color = caretColor;
            m_CursorVerts[2].color = caretColor;
            m_CursorVerts[3].color = caretColor;

            vbo.AddUIVertexQuad(m_CursorVerts);


            int screenHeight = Screen.height;
            // Removed multiple display support until it supports none native resolutions(case 741751)
            //int displayIndex = m_TextComponent.canvas.targetDisplay;
            //if (Screen.fullScreen && displayIndex < Display.displays.Length)
            //    screenHeight = Display.displays[displayIndex].renderingHeight;

            startPosition.y = screenHeight - startPosition.y;
            Input.compositionCursorPos = startPosition;
        }


        private void CreateCursorVerts()
        {
            m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < m_CursorVerts.Length; i++)
            {
                m_CursorVerts[i] = UIVertex.simpleVert;
                m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }


        private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset)
        {
            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            Debug.Log("StringPosition:" + stringPositionInternal + "  StringSelectPosition:" + stringSelectPositionInternal);

            // Adjust text RectTranform position to make sure it is visible in viewport.
            Vector2 caretPosition;
            float height = 0;
            if (caretSelectPositionInternal < textInfo.characterCount)
            {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal].origin, textInfo.characterInfo[caretSelectPositionInternal].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal].ascender - textInfo.characterInfo[caretSelectPositionInternal].descender;
            }
            else
            {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal - 1].xAdvance, textInfo.characterInfo[caretSelectPositionInternal - 1].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal - 1].ascender - textInfo.characterInfo[caretSelectPositionInternal - 1].descender;
            }

            AdjustRectTransformRelativeToViewport(caretPosition, height, true);

            int startChar = Mathf.Max(0, caretPositionInternal);
            int endChar = Mathf.Max(0, caretSelectPositionInternal);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar)
            {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;

            //Debug.Log("Updating Highlight... Caret Position: " + startChar + " Caret Select POS: " + endChar);


            //if (gen.lineCount <= 0)
            //    return;


            int currentLineIndex = textInfo.characterInfo[startChar].lineNumber;
            int nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < textInfo.characterCount)
            {
                if (currentChar == nextLineStartIdx || currentChar == endChar)
                {
                    TMP_CharacterInfo startCharInfo = textInfo.characterInfo[startChar];
                    TMP_CharacterInfo endCharInfo = textInfo.characterInfo[currentChar];
                    Vector2 startPosition = new Vector2(startCharInfo.origin, startCharInfo.ascender);
                    Vector2 endPosition = new Vector2(endCharInfo.xAdvance, endCharInfo.descender);


                    // Limit Highlight within the viewport.
                    Vector2 viewportBL = m_TextViewport.rect.min;
                    Vector2 viewportTR = m_TextViewport.rect.max;

                    float minDeltaX = (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x) - viewportBL.x;
                    if (minDeltaX < 0)
                        startPosition.x -= minDeltaX;

                    float minDeltaY = (m_TextComponent.rectTransform.anchoredPosition.y + endPosition.y) - viewportBL.y;
                    if (minDeltaY < 0)
                        endPosition.y -= minDeltaY;

                    float maxDeltaX = viewportTR.x - (m_TextComponent.rectTransform.anchoredPosition.x + endPosition.x);
                    if (maxDeltaX < 0)
                        endPosition.x += maxDeltaX;

                    float maxDeltaY = viewportTR.y - (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y);
                    if (maxDeltaY < 0)
                        startPosition.y += maxDeltaY;


                    if (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y < viewportBL.y || m_TextComponent.rectTransform.anchoredPosition.y + endPosition.y > viewportTR.y)
                    {

                    }
                    else
                    {

                        var startIndex = vbo.currentVertCount;
                        vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
                        vbo.AddVert(vert);

                        vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
                        vbo.AddVert(vert);

                        vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
                        vbo.AddVert(vert);

                        vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
                        vbo.AddVert(vert);

                        vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                        vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
                    }

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    if (currentLineIndex < textInfo.lineCount)
                        nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;
                }
                currentChar++;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="height"></param>
        /// <param name="isCharVisible"></param>
        private void AdjustRectTransformRelativeToViewport(Vector2 startPosition, float height, bool isCharVisible)
        {
            float viewportMin = m_TextViewport.rect.xMin;
            float viewportMax = m_TextViewport.rect.xMax;

            //Debug.Log("Viewport Rect: " + viewportMax + "  Start Position: " + startPosition);
            // Adjust the position of the RectTransform based on the caret position in the viewport.
            float rightOffset = viewportMax - (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x + m_TextComponent.margin.z);
            if (rightOffset < 0f)
            {
                if (!multiLine || (multiLine && isCharVisible))
                {
                    //Debug.Log("Shifting text to the right by " + rightOffset.ToString("f3"));
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(rightOffset, 0);

                    AssignPositioningIfNeeded();
                }
            }

            float leftOffset = (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x - m_TextComponent.margin.x) - viewportMin;
            if (leftOffset < 0f)
            {
                //Debug.Log("Shifting text to the left by " + leftOffset.ToString("f3"));
                m_TextComponent.rectTransform.anchoredPosition += new Vector2(-leftOffset, 0);
                AssignPositioningIfNeeded();
            }


            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float topOffset = m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y + height);
                if (topOffset < -0.0001f)
                {
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, topOffset);
                    AssignPositioningIfNeeded();
                }

                float bottomOffset = (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y) - m_TextViewport.rect.yMin;
                if (bottomOffset < 0f)
                {
                    //int currentLine = m_TextComponent.textInfo.characterInfo[Mathf.Max(0, caretSelectPositionInternal - 1)].lineNumber;
                    //bottomOffset = m_TextComponent.textInfo.lineInfo[currentLine].lineHeight;
                    m_TextComponent.rectTransform.anchoredPosition -= new Vector2(0, bottomOffset);
                    AssignPositioningIfNeeded();
                }
            }

            // Special handling of backspace
            if (m_isLastKeyBackspace)
            {
                float firstCharPosition = m_TextComponent.rectTransform.anchoredPosition.x + m_TextComponent.textInfo.characterInfo[0].origin - m_TextComponent.margin.x;
                float lastCharPosition = m_TextComponent.rectTransform.anchoredPosition.x + m_TextComponent.textInfo.characterInfo[m_TextComponent.textInfo.characterCount - 1].origin + m_TextComponent.margin.z;

                // Check if caret is at the left most position of the viewport
                if (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x <= viewportMin + 0.0001f)
                {
                    if (firstCharPosition < viewportMin)
                    {
                        float offset = Mathf.Min((viewportMax - viewportMin) / 2, viewportMin - firstCharPosition);
                        m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                        AssignPositioningIfNeeded();
                    }
                }
                else if (lastCharPosition < viewportMax && firstCharPosition < viewportMin)
                {
                    float offset = Mathf.Min(viewportMax - lastCharPosition, viewportMin - firstCharPosition);

                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    AssignPositioningIfNeeded();
                }

                m_isLastKeyBackspace = false;
            }
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>

        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = stringPositionInternal == 0 || stringSelectPositionInternal == 0;
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                    if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Name)
            {
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // Space followed by a letter -- make sure it's capitalized
                    if (char.IsLower(ch) && lastChar == ' ')
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces (and apostrophes)
                    if (char.IsUpper(ch) && lastChar != ' ' && lastChar != '\'')
                        return char.ToLower(ch);

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }
                else if (ch == ' ')
                {
                    // Don't allow more than one space in a row
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            return (char)0;
        }

        public void ActivateInputField()
        {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable())
                return;

            if (isFocused)
            {
                if (m_Keyboard != null && !m_Keyboard.active)
                {
                    m_Keyboard.active = true;
                    m_Keyboard.text = m_Text;
                }
            }

            m_HasLostFocus = false;
            m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal()
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentSelectedGameObject != gameObject)
                EventSystem.current.SetSelectedGameObject(gameObject);

            if (TouchScreenKeyboard.isSupported)
            {
                if (Input.touchSupported)
                {
                    TouchScreenKeyboard.hideInput = shouldHideMobileInput;
                }

                m_Keyboard = (inputType == InputType.Password) ?
                    TouchScreenKeyboard.Open(m_Text, keyboardType, false, multiLine, true) :
                    TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputType.AutoCorrect, multiLine);

                // Mimics OnFocus but as mobile doesn't properly support select all
                // just set it to the end of the text (where it would move when typing starts)
                MoveTextEnd(false);
            }
            else
            {
                Input.imeCompositionMode = IMECompositionMode.On;
                OnFocus();
            }

            m_AllowInput = true;
            m_OriginalText = text;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            Debug.Log("OnSelect()");

            base.OnSelect(eventData);
            ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ActivateInputField();
        }

        public void DeactivateInputField()
        {
            // Not activated do nothing.
            if (!m_AllowInput)
                return;

            m_HasDoneFocusTransition = false;
            m_AllowInput = false;

            if (m_Placeholder != null)
                m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);

            if (m_TextComponent != null && IsInteractable())
            {
                if (m_WasCanceled)
                    text = m_OriginalText;

                if (m_Keyboard != null)
                {
                    m_Keyboard.active = false;
                    m_Keyboard = null;
                }

                m_StringPosition = m_StringSelectPosition = 0;
                m_TextComponent.rectTransform.localPosition = Vector3.zero;
                if (caretRectTrans != null)
                    caretRectTrans.localPosition = Vector3.zero;

                SendOnSubmit();

                if (m_HasLostFocus)
                    SendOnFocusLost();

                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            Debug.Log("OnDeselect()");
            m_HasLostFocus = true;

            DeactivateInputField();
            base.OnDeselect(eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Debug.Log("OnSubmit()");

            if (!IsActive() || !IsInteractable())
                return;

            if (!isFocused)
                m_ShouldActivateNextUpdate = true;
        }

        //public virtual void OnLostFocus(BaseEventData eventData)
        //{
        //    if (!IsActive() || !IsInteractable())
        //        return;
        //}

        private void EnforceContentType()
        {
            switch (contentType)
            {
                case ContentType.Standard:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.Autocorrected:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.AutoCorrect;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.IntegerNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                        m_CharacterValidation = CharacterValidation.Integer;
                        return;
                    }
                case ContentType.DecimalNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                        m_CharacterValidation = CharacterValidation.Decimal;
                        return;
                    }
                case ContentType.Alphanumeric:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                        m_CharacterValidation = CharacterValidation.Alphanumeric;
                        return;
                    }
                case ContentType.Name:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.Name;
                        return;
                    }
                case ContentType.EmailAddress:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                        m_CharacterValidation = CharacterValidation.EmailAddress;
                        return;
                    }
                case ContentType.Password:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.Pin:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                        m_CharacterValidation = CharacterValidation.Integer;
                        return;
                    }
                default:
                    {
                        // Includes Custom type. Nothing should be enforced.
                        return;
                    }
            }
        }


        void SetTextComponentWrapMode()
        {
            if (m_TextComponent == null)
                return;

            if (m_LineType == LineType.SingleLine)
                m_TextComponent.enableWordWrapping = false;
            else
                m_TextComponent.enableWordWrapping = true;
        }

        // Control Rich Text option on the text component.
        void SetTextComponentRichTextMode()
        {
            if (m_TextComponent == null)
                return;

            m_TextComponent.richText = m_RichText;
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            if (contentType == ContentType.Custom)
                return;

            for (int i = 0; i < allowedContentTypes.Length; i++)
                if (contentType == allowedContentTypes[i])
                    return;

            contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            if (contentType == ContentType.Custom)
                return;

            contentType = ContentType.Custom;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (m_HasDoneFocusTransition)
                state = SelectionState.Highlighted;
            else if (state == SelectionState.Pressed)
                m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }
    }



    static class SetPropertyUtility
    {
        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}