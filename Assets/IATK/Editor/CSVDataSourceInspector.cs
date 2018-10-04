using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IATK
{
    [CustomEditor(typeof(CSVDataSource))]
    [CanEditMultipleObjects]
    public class CSVDataSourceInspector : InspectorChangeEditor
    {
    }

}   // Namespace