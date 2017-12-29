using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceAcademy : Academy {
	[HideInInspector]
	public float roadWidth = 90.0f;

	public override void AcademyReset()
	{
		roadWidth = (float)resetParameters["road_width"];
	}

	public override void AcademyStep()
	{


	}

}
