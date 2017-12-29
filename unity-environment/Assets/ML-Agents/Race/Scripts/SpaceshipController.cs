using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipController : MonoBehaviour {
	public ParticleSystem trailParticle;
	public TrailRenderer trailRenderer;
	public float accelerate;
	public float dragRate;
	public float maxAngularSpeed;
	public bool goal;

	private Rigidbody _rb;

	private bool _boost;
	private int _turnDir;
	// Use this for initialization
	void Start () {
		if (_rb == null)
			_rb = GetComponent<Rigidbody>();
		Boost(false);
		SetTurning(0);
	}
	
	// Update is called once per frame
	void Update () {
		if (_rb == null)
			return;
		Vector3 powerForce = Vector3.zero;
		if (_boost)
		{
			Vector3 faceDir = transform.forward.normalized;
			powerForce = faceDir * accelerate;
		}

		float speedValue = _rb.velocity.magnitude;

		Vector3 dragForce = _rb.velocity.normalized * -1.0f * speedValue * dragRate;

		_rb.AddForce(powerForce + dragForce);

		if (_turnDir != 0)
		{
			float turnAngle = _turnDir * maxAngularSpeed * Time.deltaTime;
			transform.Rotate(new Vector3(0.0f, turnAngle, 0.0f));
		}
	}

	public void Boost(bool b)
	{
		_boost = b;
		if (b)
			trailParticle.Play();
		else
			trailParticle.Stop();
	}

	public void SetTurning(int t)
	{
		if (t > 0)
			_turnDir = 1;
		else if (t < 0)
			_turnDir = -1;
		else
			_turnDir = 0;
	}

	public void Stop()
	{
		if (_rb != null)
			_rb.velocity = Vector3.zero;
	}

	public Vector3 GetVelocity()
	{
		Vector3 result = Vector3.zero;
		if (_rb != null)
			result = _rb.velocity;

		return result;
	}

	public float GetYaw()
	{
		return transform.eulerAngles.y;
	}

	public void Reset()
	{
		trailRenderer.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log(other.gameObject.name);
		if (other.gameObject.tag == "Goal")
			goal = true;
	}
}
