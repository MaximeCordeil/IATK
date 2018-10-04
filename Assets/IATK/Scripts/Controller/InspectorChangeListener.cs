using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{

    /// <summary>
    /// Inspector change listener. Derive from this to receive inspector change events
    /// </summary>
    public interface InspectorChangeListener 
    {
        void onInspectorChange();
    }

}   // Namespace
