using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisAnchor : MonoBehaviour {

    Axis axis { get { return transform.GetComponentInParent<Axis>(); } }
    
    void OnTriggerEnter(Collider col)
    {
        axis.ConnectedAxis.Add(col.GetComponent<AxisAnchor>().axis);
        //axis.transform.Find("axis_mesh/Box").GetComponent<Renderer>().material.color = Color.red;        
    }

    void OnTriggerExit(Collider col)
    {
        axis.ConnectedAxis.Remove(col.GetComponent<AxisAnchor>().axis);
        if (axis.ConnectedAxis.Count == 0)
        {
            //axis.transform.Find("axis_mesh/Box").GetComponent<Renderer>().material.color = Color.white;    
        }
    }
}
