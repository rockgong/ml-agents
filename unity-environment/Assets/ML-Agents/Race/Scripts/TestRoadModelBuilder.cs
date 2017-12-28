using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModelBuildCommand
{
	public int type;
	public float val;
}

public class TestRoadModelBuilder : MonoBehaviour {
	public RoadModelBuilder builder;

	public ModelBuildCommand[] commands;

	private void OnGUI()
	{
		if (GUILayout.Button("Build"))
		{
			List<RoadModelBuilder.Command> cmds = new List<RoadModelBuilder.Command>();
			for (int i = 0, n = commands.Length; i < n; i++)
				cmds.Add(new RoadModelBuilder.Command(commands[i].type, commands[i].val));

			builder.Build(cmds);
		}
	}
}
