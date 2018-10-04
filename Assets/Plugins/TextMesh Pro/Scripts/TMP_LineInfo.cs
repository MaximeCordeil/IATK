// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


namespace TMPro
{

    /// <summary>
    /// Structure which contains information about the individual lines of text.
    /// </summary>
    public struct TMP_LineInfo
    {
        public int characterCount;
        public int visibleCharacterCount;
        public int spaceCount;
        public int wordCount;
        public int firstCharacterIndex;
        public int firstVisibleCharacterIndex;
        public int lastCharacterIndex;
        public int lastVisibleCharacterIndex;

        public float length;
        public float lineHeight;
        public float ascender;
        public float baseline;
        public float descender;
        public float maxAdvance;

        public float width;
        public float marginLeft;
        public float marginRight;

        public TextAlignmentOptions alignment;
        public Extents lineExtents;
    }
}