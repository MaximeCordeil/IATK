using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooktAtCamera : MonoBehaviour {

    public GameObject VR_Camera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if(VR_Camera!=null)
        transform.LookAt(VR_Camera.transform);

	}
}
