using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TemplateAgent : Agent {
	public SpaceshipController spaceShip;
	public RoadModelBuilder roadBuilder;
	public Transform goalTrans;

	public float roadLength = 100.0f;
	public float minRoadSegmentLength = 10.0f;
	public float maxRoadSegmentLength = 80.0f;
	public float minRoadRadius = 12.0f;
	public float maxRoadRadius = 30.0f;
	public float detectStep = 1.0f;
	public int detectCount = 5;

	private int processBlock;
	private int totalBlock;
	private int processBlockLastStep;

	private float[] _detection = null;

	public override List<float> CollectState()
	{
		if (_detection == null)
			_detection = new float[detectCount];
		float pX = spaceShip.transform.position.x;
		float pY = spaceShip.transform.position.z;
		float fX = spaceShip.transform.forward.x;
		float fY = spaceShip.transform.forward.z;
		for (int i = 0; i < detectCount; i++)
		{
			roadBuilder.QueryProgress(pX, pY, (p, c) =>
			{
				_detection[i] = (float)p / c;
			});
			pX += fX * detectStep;
			pY += fY * detectStep;
		}

		List<float> state = new List<float>();
		state.Add(spaceShip.transform.position.x);
		state.Add(spaceShip.transform.position.z);
		for (int i = 0; i < detectCount; i++)
		{
			state.Add(_detection[i]);
			Monitor.Log(string.Format("Detection_{0}", i), _detection[i].ToString());
		}
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
		float rewardPerProcess = 1.0f / detectCount;
		for (int i = 1; i < detectCount; i++)
		{
			if (_detection[i] == _detection[0])
				continue;

			if (_detection[i] > _detection[0])
			{
				if (boost)
					reward = 0.9f;
				else
					reward = 0.5f;
				break;
			}
			else
			{
				if (boost)
					reward = -0.1f;
				else
					reward = 0.3f;
				break;
			}
		}
		reward *= _detection[0] + 0.01f;
		if (spaceShip.goal)
		{
			done = true;
			reward = 1.0f;
		}
		else if (_detection[0] < 0.0f)
		{
			done = true;
			reward = -1.0f;
		}

        Monitor.Log("Reward", reward, MonitorType.slider, Camera.main.transform);
        Monitor.Log("Action", string.Format("{0},{1}", act[0], act[1]), MonitorType.text, Camera.main.transform);

		processBlockLastStep = processBlock;

	}

	public override void AgentReset()
	{
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
