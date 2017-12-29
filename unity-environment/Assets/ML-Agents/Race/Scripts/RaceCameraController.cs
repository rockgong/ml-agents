using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCameraController : MonoBehaviour {
	public float moveVelocity = 1.0f;
	public float moveVelocityDistanceFactor = 0.05f;
	public float distanceSpeed = 1.0f;
	public float minDistance = 0.5f;
	public float maxDistance = 2.0f;

	private Vector3 _preMousePos = Vector3.zero;

	private Vector2 _mouseScrollDelta = new Vector2(0.0f, 0.0f);

	private void Start()
	{
		_preMousePos = Input.mousePosition;
	}

	private void LateUpdate()
	{
		Vector3 curMousePos = Input.mousePosition;

		if (Input.GetMouseButton(0))
		{
			Vector3 deltaMousePos = curMousePos - _preMousePos;
			Vector3 movement = new Vector3(deltaMousePos.x, 0.0f, deltaMousePos.y);
			transform.Translate((curMousePos - _preMousePos) * (-moveVelocity) * Time.deltaTime * transform.position.y * moveVelocityDistanceFactor);
		}

		float posY = transform.position.y;
		_mouseScrollDelta = Input.mouseScrollDelta;
		posY += -_mouseScrollDelta.y * distanceSpeed;
		posY = Mathf.Clamp(posY, minDistance, maxDistance);
		transform.position = new Vector3(transform.position.x, posY, transform.position.z);

		_preMousePos = curMousePos;
	}
}
