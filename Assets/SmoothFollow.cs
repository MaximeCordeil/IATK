using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {

    public Transform target;

    public float speed = 2.0f;
    public bool dampen;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            dampen = !dampen;
        }
    }

   
	// Update is called once per frame
	void LateUpdate() {

        if (dampen)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * speed);       
        } else
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
	}
}
