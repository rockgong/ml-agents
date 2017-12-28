using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour {
	public Transform followTarget;
	public Vector3 followOffset;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = followTarget.position + followOffset;
		transform.LookAt(followTarget.position);
	}
}
