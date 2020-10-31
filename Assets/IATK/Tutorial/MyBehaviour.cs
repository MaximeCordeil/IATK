using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBehaviour : MonoBehaviour {

    //my rotation
    public Vector3 rotation; 

    //speed: explain
    public float speed = 1f;

	// Use this for initialization
	void Start () {

        rotation = Vector3.up;

	}
	
	// Update is called once per frame
	void Update () {

        // Update the position of the object
        // transform.position = new Vector3((Input.GetAxis("Horizontal")), (Input.GetAxis("Vertical")), 0f);
        transform.position = new Vector3(0f, Mathf.Sin(Time.time * speed),0f);

        // Update the rotation of the object
        transform.Rotate(rotation, Time.deltaTime * speed);

        // Update the rotation of the object
        transform.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.blue, Mathf.Sin(Time.time * speed));
	}
}
