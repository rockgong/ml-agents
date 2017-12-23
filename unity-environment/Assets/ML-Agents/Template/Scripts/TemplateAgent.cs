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

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();

		state.Add(spaceShip.transform.position.x);
		state.Add(spaceShip.transform.position.y);
		state.Add(spaceShip.transform.position.z);

		state.Add(spaceShip.GetVelocity().magnitude);
		state.Add(spaceShip.GetYaw());

		return state;
	}

	public override void AgentStep(float[] act)
	{
		bool boost = ((int)act[0]) % 2 == 1;
		int turnDir = 0;
		int val = ((int)act[0]) / 2;
		if (val % 2 == 1)
			turnDir -= 1;
		else if ((val / 2) % 2 == 1)
			turnDir += 1;

		if (spaceShip != null)
		{
			spaceShip.Boost(boost);
			spaceShip.SetTurning(turnDir);
		}

		done = spaceShip != null ? spaceShip.goal : false;
		if (done)
			reward = 1.0f;
		else
		{
			reward = 1.0f - Mathf.Atan((goalTrans.position - spaceShip.transform.position).magnitude) / (Mathf.PI / 2);
		}
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

	public IEnumerable<RoadModelBuilder.Command> RoadModelBuilderCommandGenerate(float totalLength, float minSegLength, float maxSegLength, float minRadius, float maxRadius)
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

	void OnGUI()
	{
		GUILayout.Label(reward.ToString());
	}
}
