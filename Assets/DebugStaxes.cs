using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugStaxes : MonoBehaviour {

	// Use this for initialization
	void Start () {

        GameObject[] axes = GameObject.FindGameObjectsWithTag("Axis");

        axes[3].transform.Translate(-0.5f, 0f, 0f);
        axes[3].transform.Rotate(0f, 0f, -90);
        axes[3].transform.Translate(0f, -0.5f, 0f);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
