using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMeshMiddlePoint : MonoBehaviour {
	public RoadModelBuilder roadModelBuilder;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = roadModelBuilder.GetBoundsCenter();
	}
}
