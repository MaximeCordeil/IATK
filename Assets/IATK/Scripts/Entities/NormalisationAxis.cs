using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    public class NormalisationAxis : MonoBehaviour {

        public GameObject end;
        Axis myAxis;

        // Use this for initialization
        void Start () {
            myAxis = GetComponentInParent<Axis>();

        }
        
        // Update is called once per frame
        void Update () {
            var lr = GetComponent<LineRenderer>();
            lr.SetPosition(0, new Vector3(0, 0, -myAxis.MaxNormaliser));
            lr.SetPosition(1, new Vector3(0, 0, -myAxis.MinNormaliser));
        }
    }
}
