using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoView : MonoBehaviour {
	public RaceAgent raceAgent;
	public RoadModelBuilder roadBuilder;
	public Brain brain;

	public InputField lengthText;
	public InputField widthText;

	public Text brainTypeText;

	private void Start()
	{
		lengthText.text = raceAgent.roadLength.ToString();
		widthText.text = roadBuilder.roadWidth.ToString();
		brainTypeText.text = GetButtonStringFromBrainType(brain.brainType);
	}

	public void OnLengthTextChanged(string text)
	{
		float parseResult = 0.0f;
		if (float.TryParse(text, out parseResult))
		{
			raceAgent.roadLength = Mathf.Clamp(parseResult, 20.0f, 300.0f);
			lengthText.text = raceAgent.roadLength.ToString();
		}
	}

	public void OnWidthTextChagned(string text)
	{
		float parseResult = 0.0f;
		if (float.TryParse(text, out parseResult))
		{
			roadBuilder.roadWidth = Mathf.Clamp(parseResult, 3.0f, 15.0f);
			widthText.text = roadBuilder.roadWidth.ToString();
		}
	}

	private string GetButtonStringFromBrainType(BrainType brainType)
	{
		if (brainType == BrainType.Player)
			return "Player";
		else if (brainType == BrainType.Internal)
			return "AI";
		return string.Empty;
	}

	public void OnChangeBrainType()
	{
		BrainType bt = brain.brainType == BrainType.Player ? BrainType.Internal : BrainType.Player;
		brain.brainType = bt;
		brain.UpdateCoreBrains();
		brainTypeText.text = GetButtonStringFromBrainType(brain.brainType);
	}
}
