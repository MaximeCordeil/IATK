using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
public class DistanceLinker : MonoBehaviour {

    public LinkingVisualisations lv;
    public float distance = 0.5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (lv!=null)
            lv.showLinks = Vector3.Distance(lv.visualisationSource.transform.position, lv.visualisationTarget.transform.position) < distance;
		
	}
}
