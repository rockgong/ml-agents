using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateDecision : MonoBehaviour, Decision
{

    public float[] Decide(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
    {
    	float[] result = new float[1]{0};
    	if (state[2] < 11.0f || state[4] > 90.0f)
        	result[0] = 1;
        else
        	result[0] = 5;
        return result;
    }

    public float[] MakeMemory(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
    {
        return new float[0];
		
    }
}
