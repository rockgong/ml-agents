using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class RaceAgent : Agent {
	public SpaceshipController spaceShip;
	public RoadModelBuilder roadBuilder;
	public Transform goalTrans;
	public RaceAcademy raceAcademy;

	public float roadLength = 100.0f;
	public float minRoadSegmentLength = 10.0f;
	public float maxRoadSegmentLength = 80.0f;
	public float minRoadRadius = 12.0f;
	public float maxRoadRadius = 30.0f;
	public float detectStep = 1.0f;
	public int detectCount = 5;
	public float detectAngleStep = 1.0f;
	public float turnThreshold = 0.2f;

	private int processBlock;
	private int totalBlock;
	private int processBlockLastStep;

	private float detection0 = 0.0f;
	private float dirDetection = 0.0f;
	private float dirDetectionLeft = 0.0f;
	private float dirDetectionRight = 0.0f;

	private float[] _currentDetection = null;

	private void LoadAcademyParameter()
	{
		if (raceAcademy != null)
			roadBuilder.roadWidth = raceAcademy.roadWidth;
	}

	private float GetDirDetection(float baseX, float baseY, float forwardX, float forwardY)
	{
		if (_currentDetection == null)
			_currentDetection = new float[detectCount];
		float pX = baseX;
		float pY = baseY;
		int detectGoal = -1;
		bool detectOuter = false;

		for (int i = 0; i < detectCount; i++)
		{
			if (detectGoal < 0)
			{
				if (!detectOuter)
				{
					Vector3 rayOrigin = new Vector3(pX, 0.0f, pY) + Vector3.up * 5.0f;
					Vector3 rayDirection = Vector3.down;
					RaycastHit hit = new RaycastHit();
					if (Physics.Raycast(rayOrigin, rayDirection, out hit))
					{
						if (hit.collider.gameObject.tag == "Goal")
							detectGoal = i;
					}
				}

				roadBuilder.QueryProgress(pX, pY, (p, c) =>
				{
					_currentDetection[i] = (float)p / c;
				});
			}
			else
				_currentDetection[i] = 1.0f + (i - detectGoal) * 0.1f;

			if (_currentDetection[i] < 0.0f)
				detectOuter = true;
			pX += forwardX * detectStep;
			pY += forwardY * detectStep;
		}

		float result = 0.0f;
		for (int i = 0; i < detectCount; i++)
		{
			if (_currentDetection[i] >= 0.0f)
				result += _currentDetection[i] - _currentDetection[0];
			else
				break;
		}

		return result;
	}

	private float DetectOneHandSide(float baseX, float baseY, float startForwardX, float startForwardY, float angleStep, int count)
	{
		float result = -99999999.0f;
		Quaternion rotationStep = Quaternion.Euler(0.0f, angleStep, 0.0f);
		Vector3 curDir = new Vector3(startForwardX, 0.0f, startForwardY);
		for (int i = 0; i < count; i++)
		{
			curDir = rotationStep * curDir;
			float stepResult = GetDirDetection(baseX, baseY, curDir.x, curDir.z);
			if (stepResult > result)
				result = stepResult;
		}
		return result;
	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		float pX = spaceShip.transform.position.x;
		float pY = spaceShip.transform.position.z;
		roadBuilder.QueryProgress(pX, pY, (p, c) =>
		{
			detection0 = (float)p / c;
		});
		float fX = spaceShip.transform.forward.x;
		float fY = spaceShip.transform.forward.z;

		dirDetection = GetDirDetection(pX, pY, fX, fY);
		state.Add(dirDetection);
		Monitor.Log("DirDect", dirDetection.ToString());

		int detectStepCount = (int)(180.0f / detectAngleStep) + 1;
		dirDetectionLeft = DetectOneHandSide(pX, pY, fX, fY, -detectAngleStep, detectCount);
		dirDetectionRight = DetectOneHandSide(pX, pY, fX, fY, detectAngleStep, detectCount);
		state.Add(dirDetectionLeft);
		Monitor.Log("DirDectL", dirDetectionLeft.ToString());
		state.Add(dirDetectionRight);
		Monitor.Log("DirDectR", dirDetectionRight.ToString());

		state.Add(spaceShip.GetVelocity().x);
		state.Add(spaceShip.GetVelocity().z);
		state.Add(Mathf.Sin(spaceShip.GetYaw()));
		state.Add(Mathf.Cos(spaceShip.GetYaw()));
		Monitor.Log("Speed", spaceShip.GetVelocity().magnitude);
		Monitor.Log("Yaw", spaceShip.GetYaw());
		return state;
	}

	public override void AgentStep(float[] act)
	{
		bool boost = act[0] > 0.0f;
		int turnDir = 0;
		if (act[1] > 0.25f)
			turnDir = 1;
		else if (act[1] < -0.25f)
			turnDir = -1;

		if (spaceShip != null)
		{
			spaceShip.Boost(boost);
			spaceShip.SetTurning(turnDir);
		}

		done = spaceShip != null ? spaceShip.goal : false;
		reward = 0.0f;
		if (dirDetection > 0.0f && dirDetection + turnThreshold >= dirDetectionLeft && dirDetection + turnThreshold >= dirDetectionRight)
		{
			if (turnDir == 0)
				reward += 0.5f;
		}
		else
		{
			if (dirDetectionLeft > dirDetectionRight)
			{
				if (turnDir == -1)
					reward += 0.1f;
				else if (turnDir == 1)
					reward -= 0.5f;
			}
			else
			{
				if (turnDir == 1)
					reward += 0.1f;
				else if (turnDir == -1)
					reward -= 0.5f;
			}
		}
		if (dirDetection > 0.0f)
		{
			if (boost)
				reward += 0.5f;
		}
		else if (dirDetection < 0.0f)
		{
			if (boost)
				reward -= 0.5f;
		}

        Monitor.Log("Reward", reward, MonitorType.slider, Camera.main.transform);
        Monitor.Log("Action", string.Format("{0},{1}", act[0], act[1]), MonitorType.text, Camera.main.transform);

		processBlockLastStep = processBlock;

	}

	public override void AgentReset()
	{
		LoadAcademyParameter();
		if (spaceShip != null)
		{
			spaceShip.Boost(false);
			spaceShip.SetTurning(0);
			spaceShip.transform.position = Vector3.zero;
			spaceShip.transform.rotation = Quaternion.identity;
			spaceShip.goal = false;
			spaceShip.Stop();
		}

		if (roadBuilder != null)
		{
			List<RoadModelBuilder.Command> commandList = new List<RoadModelBuilder.Command>();
			foreach(RoadModelBuilder.Command cmd in RoadModelBuilderCommandGenerate(roadLength, minRoadSegmentLength, maxRoadSegmentLength, minRoadRadius, maxRoadRadius))
				commandList.Add(cmd);
			roadBuilder.Build(commandList, (x, y, ga) =>
			{
				goalTrans.position = new Vector3(x, 0.0f, y);
				goalTrans.rotation = Quaternion.Euler(0.0f, -ga / Mathf.PI * 180.0f, 0.0f);
				goalTrans.localScale = new Vector3(roadBuilder.roadWidth, 1.0f, goalTrans.localScale.z);
			});
		}
	}

	public override void AgentOnDone()
	{

	}

	private IEnumerable<RoadModelBuilder.Command> RoadModelBuilderCommandGenerate(float totalLength, float minSegLength, float maxSegLength, float minRadius, float maxRadius)
	{
		float length = totalLength;
		float curT = 0.0f;
		float cnt = Random.value > 0.5f ? 1 : 0;
		while(true)
		{
			float curDeltaT = (cnt % 2 == 0 ? 1.0f : -1.0f) * (Random.value * (maxRadius - minRadius) + minRadius) - curT;
			curT += curDeltaT;
			cnt++;
			yield return new RoadModelBuilder.Command(0, curDeltaT);
			float curLength = Random.value * (maxSegLength - minSegLength) + minSegLength;
			if (length <= curLength)
				break;
			length -= curLength;
			yield return new RoadModelBuilder.Command(1, curLength);
		}
		yield return new RoadModelBuilder.Command(1, length);
	}
}
