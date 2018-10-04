using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;


namespace IATK
{
    [Serializable]
    public class AttributeFilter
    {
        [Tooltip("The name of the attribute")]
        public string Attribute = "Undefined";

        [Tooltip("Minimum filter value for the attribute")]
        [Range(0.0f, 1.0f)]
        public float minFilter = 0.0f;

        [Tooltip("Maximum filter value for the attribute")]
        [Range(0.0f, 1.0f)]
        public float maxFilter = 1.0f;

        [Tooltip("Minimum scaling value for the attribute")]
        [Range(0.0f, 1.0f)]
        public float minScale = 0.0f;

        [Tooltip("Maximum scaling value for the attribute")]
        [Range(0.0f, 1.0f)]
        public float maxScale = 1.0f;

        public static implicit operator AttributeFilter(string str)
        {
            var a = new AttributeFilter
            {
                Attribute = str
            };
            return a;
        }
    }

    [Serializable]
    public class DimensionFilter : AttributeFilter
    {
        public static implicit operator DimensionFilter(string str)
        {
            var a = new DimensionFilter
            {
                Attribute = str
            };
            return a;
        }
    }
}