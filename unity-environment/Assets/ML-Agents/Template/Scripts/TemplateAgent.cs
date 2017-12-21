using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TemplateAgent : Agent {
	public SpaceshipController spaceShip;
	public RoadModelBuilder roadBuilder;
	public Transform goalTrans;

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
			commandList.Add(new RoadModelBuilder.Command(0, 10));
			commandList.Add(new RoadModelBuilder.Command(1, 10));
			commandList.Add(new RoadModelBuilder.Command(0, -20));
			commandList.Add(new RoadModelBuilder.Command(1, 10));
			commandList.Add(new RoadModelBuilder.Command(0, -20));
			commandList.Add(new RoadModelBuilder.Command(1, 30));
			roadBuilder.Build(commandList);
		}
	}

	public override void AgentOnDone()
	{

	}

	void OnGUI()
	{
		GUILayout.Label(reward.ToString());
	}
}
