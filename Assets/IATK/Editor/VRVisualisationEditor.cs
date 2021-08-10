using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IATK
{
    [CustomEditor(typeof(VRVisualisation))]
    [CanEditMultipleObjects]
    public class VRVisualisationEditor : AbstractVisualisationEditor
    {
        [MenuItem("GameObject/IATK/VR Visualisation", false, 10)]
        static void CreateVRVisualisationPrefab()
        {
            GameObject obj = new GameObject("[IATK] New VR Visualisation");
            obj.AddComponent<VRVisualisation>();
            Selection.activeGameObject = obj;
        }
    }

}   // Namespace