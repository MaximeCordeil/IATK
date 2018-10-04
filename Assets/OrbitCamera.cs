using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour {

    public bool orbit;
    public float speed = 1.0f;
    public Transform target;

    // Update is called once per frame
    void LateUpdate () {
        if (target != null && orbit)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);        
            transform.LookAt(target);
        }
    }
}
