using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IATK
{
    
    [CreateAssetMenuAttribute(menuName="IATK/Data Metadata")]
    public class DataMetadata : ScriptableObject {

        [Serializable]
        public struct BinSize 
        {
            public int index;
            public int binCount;
        }

        [Tooltip("Override a particular dimension's bin size")]    
        public List<BinSize> BinSizePreset = new List<BinSize>();

        [Tooltip("index of the dimension that will categorize the color")]
        public int dimensionColor = 0;
    }

}   // Namespace