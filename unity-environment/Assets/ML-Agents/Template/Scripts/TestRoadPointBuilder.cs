using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRoadPointBuilder : MonoBehaviour {
	public float initPX;
	public float initPY;
	public float initNA;
	public float initC0;
	public float initC1;
	public float initStep;

	private RoadPointBuilder _builder = null;

	private string changePivotT = string.Empty;
	private string buildDistance = string.Empty;
	private string testPointX = string.Empty;
	private string testPointY = string.Empty;
	private Vector2 _scrollVec = new Vector2(0.0f, 0.0f);

	private Mesh _targetMesh;
	// Use this for initialization
	void Start () {
		_builder = new RoadPointBuilder(initPX, initPY, initNA, initC0, initC1, initStep);

		MeshFilter mf = gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();
		_targetMesh = new Mesh();
		mf.mesh = _targetMesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI()
	{
		GUILayout.BeginHorizontal();
		changePivotT = GUILayout.TextField(changePivotT, GUILayout.Width(100.0f));
		if (GUILayout.Button("ChangePovit"))
		{
			_builder.ChangePivot(float.Parse(changePivotT));
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		buildDistance = GUILayout.TextField(buildDistance, GUILayout.Width(100.0f));
		if (GUILayout.Button("Build"))
		{
			_builder.BuildDistance(float.Parse(buildDistance));
			UpdateMesh();
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		testPointX = GUILayout.TextField(testPointX, GUILayout.Width(100.0f));
		testPointY = GUILayout.TextField(testPointY, GUILayout.Width(100.0f));
		if (GUILayout.Button("Test"))
		{
			float px = float.Parse(testPointX);
			float py = float.Parse(testPointY);
			int index = _builder.GetInsectBlock(px, py);
			int count = _builder.GetBlockCount();
			Debug.Log(string.Format("{0} / {1}", index, count));
			Debug.DrawRay(new Vector3(px, 0.0f, py), Vector3.up, Color.white, 1.0f);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		_builder.QueryCursorState((px, py, ga, c0, c1) =>
		{
			GUILayout.Label(px.ToString(), GUILayout.Width(80.0f));
			GUILayout.Label(py.ToString(), GUILayout.Width(80.0f));
			GUILayout.Label(ga.ToString(), GUILayout.Width(80.0f));
			GUILayout.Label(c0.ToString(), GUILayout.Width(80.0f));
			GUILayout.Label(c1.ToString(), GUILayout.Width(80.0f));
		});
		GUILayout.EndHorizontal();
	}

	private void UpdateMesh()
	{
		List<Vector3> vertList = new List<Vector3>();
		List<int> triList = new List<int>();
		int groupCount = -1;
		_builder.ForEachRoadPoint((p0, p1) =>
		{
			vertList.Add(new Vector3(p0.x, 0.0f, p0.y));
			vertList.Add(new Vector3(p1.x, 0.0f, p1.y));
			if (groupCount >= 0)
			{
				triList.Add(groupCount * 2 + 0);
				triList.Add(groupCount * 2 + 2);
				triList.Add(groupCount * 2 + 1);
				triList.Add(groupCount * 2 + 1);
				triList.Add(groupCount * 2 + 2);
				triList.Add(groupCount * 2 + 3);
			}
			groupCount++;
		});
		_targetMesh.vertices = vertList.ToArray();
		_targetMesh.triangles = triList.ToArray();
	}
}
