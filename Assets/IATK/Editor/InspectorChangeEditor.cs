using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Linq;

namespace IATK
{

    /// <summary>
    /// An editor to raise callbacks when a property is changed in the inspector
    /// </summary>
    public abstract class InspectorChangeEditor : Editor  
    {

        void OnEnable()
        {

        }

        /// <summary>
        /// Draw the inspector and update Visualisation when a property changes
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
                                      
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                InspectorChangeListener target = (InspectorChangeListener)serializedObject.targetObject;
                if (target != null)
                {
                    target.onInspectorChange();
                }
            }
        }      
            
    }

}   // Namespace
