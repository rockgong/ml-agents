using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadModelBuilder : MonoBehaviour {
	public struct Command
	{
		public int t;
		public float v;

		public Command(int type, float val)
		{
			t = type;
			v = val;
		}
	}

	private Mesh _mesh;
	private Mesh _colliderMesh0;
	private Mesh _colliderMesh1;
	private MeshCollider _meshCol0;
	private MeshCollider _meshCol1;

	public float roadWidth = 2.0f;
	public float roadHeight = 2.0f;
	public float roadThickness = 0.5f;
	public float stepLength = 0.2f;

	private Vector3 _center = Vector3.zero;
	// Use this for initialization
	void Start () {
		_mesh = GetComponent<MeshFilter>().mesh;
		if (_mesh == null)
		{
			_mesh = new Mesh();
			_mesh.name = "RoadMesh";
			GetComponent<MeshFilter>().mesh = _mesh;
		}

		GameObject go = null;

		go = new GameObject("Col0");
		go.transform.SetParent(transform);
		_meshCol0 = go.AddComponent<MeshCollider>();
		_colliderMesh0 = new Mesh();
		_colliderMesh0.name = "Col0Mesh";

		go = new GameObject("Gol1");
		go.transform.SetParent(transform);
		_meshCol1 = go.AddComponent<MeshCollider>();
		_colliderMesh1 = new Mesh();
		_colliderMesh1.name = "Col1Mesh";
	}

	public void Build(List<Command> commands, System.Action<float, float, float> onEnd = null)
	{
		RoadPointBuilder pointBuilder = new RoadPointBuilder(0.0f, 0.0f, 0.0f, -roadWidth / 2, roadWidth / 2, stepLength);

		float endX = 0.0f, endY = 0.0f, endGA = 0.0f;

		for (int i = 0, n = commands.Count; i < n; i++)
		{

			if (commands[i].t == 0)
				pointBuilder.ChangePivot(commands[i].v);
			else if (commands[i].t == 1)
				pointBuilder.BuildDistance(commands[i].v);
		}
		pointBuilder.QueryCursorState((x, y, ga, c0, c1) =>
		{
			pointBuilder.GetCursorMiddlePoint(out endX, out endY);
			endGA = ga;
		});

		List<Vector3> vertList = new List<Vector3>();
		List<Vector3> col0VertList = new List<Vector3>();
		List<Vector3> col1VertList = new List<Vector3>();
		List<int> triList = new List<int>();
		List<int> col0TriList = new List<int>();
		List<int> col1TriList = new List<int>();
		int groupCount = -1;
		float minX = 0.0f,
			maxX = 0.0f,
			minY = 0.0f,
			maxY = 0.0f,
			minZ = 0.0f,
			maxZ = 0.0f;
		pointBuilder.ForEachRoadPoint((p0, p1) =>
		{
			Vector3 orient = (new Vector3(p1.x - p0.x, 0.0f, p1.y - p0.y)).normalized;

			Vector3 v0 = new Vector3(p0.x, 0.0f, p0.y);
			Vector3 v1 = new Vector3(p1.x, 0.0f, p1.y);
			Vector3 v2 = v0 + Vector3.up * roadHeight;
			Vector3 v3 = v1 + Vector3.up * roadHeight;
			Vector3 v4 = v2 - orient * roadThickness;
			Vector3 v5 = v3 + orient * roadThickness;
			Vector3 v6 = v4 - Vector3.up * (roadHeight + roadThickness);
			Vector3 v7 = v5 - Vector3.up * (roadHeight + roadThickness);

			float[] xs = new float[]{v0.x, v1.x, v2.x, v3.x, v4.x, v5.x, v6.x, v7.x};
			float[] ys = new float[]{v0.y, v1.y, v2.y, v3.y, v4.y, v5.y, v6.y, v7.y};
			float[] zs = new float[]{v0.z, v1.z, v2.z, v3.z, v4.z, v5.z, v6.z, v7.z};

			for (int i = 0, n = xs.Length; i < n; i++)
			{
				if (xs[i] < minX)
					minX = xs[i];
				if (xs[i] > maxX)
					maxX = xs[i];
			}
			for (int i = 0, n = ys.Length; i < n; i++)
			{
				if (ys[i] < minY)
					minY = ys[i];
				if (ys[i] > maxY)
					maxY = ys[i];
			}
			for (int i = 0, n = zs.Length; i < n; i++)
			{
				if (zs[i] < minZ)
					minZ = zs[i];
				if (zs[i] > maxZ)
					maxZ = zs[i];
			}

			vertList.Add(v0);
			vertList.Add(v1);
			vertList.Add(v2);
			vertList.Add(v3);
			vertList.Add(v4);
			vertList.Add(v5);
			vertList.Add(v6);
			vertList.Add(v7);

			if (groupCount >= 0)
			{
				triList.Add(groupCount * 8 + 0);
				triList.Add(groupCount * 8 + 8);
				triList.Add(groupCount * 8 + 1);
				triList.Add(groupCount * 8 + 8);
				triList.Add(groupCount * 8 + 9);
				triList.Add(groupCount * 8 + 1);

				triList.Add(groupCount * 8 + 0);
				triList.Add(groupCount * 8 + 2);
				triList.Add(groupCount * 8 + 8);
				triList.Add(groupCount * 8 + 2);
				triList.Add(groupCount * 8 + 10);
				triList.Add(groupCount * 8 + 8);

				triList.Add(groupCount * 8 + 1);
				triList.Add(groupCount * 8 + 9);
				triList.Add(groupCount * 8 + 3);
				triList.Add(groupCount * 8 + 9);
				triList.Add(groupCount * 8 + 11);
				triList.Add(groupCount * 8 + 3);

				triList.Add(groupCount * 8 + 2);
				triList.Add(groupCount * 8 + 4);
				triList.Add(groupCount * 8 + 10);
				triList.Add(groupCount * 8 + 4);
				triList.Add(groupCount * 8 + 12);
				triList.Add(groupCount * 8 + 10);

				triList.Add(groupCount * 8 + 3);
				triList.Add(groupCount * 8 + 11);
				triList.Add(groupCount * 8 + 5);
				triList.Add(groupCount * 8 + 11);
				triList.Add(groupCount * 8 + 13);
				triList.Add(groupCount * 8 + 5);

				triList.Add(groupCount * 8 + 4);
				triList.Add(groupCount * 8 + 6);
				triList.Add(groupCount * 8 + 12);
				triList.Add(groupCount * 8 + 6);
				triList.Add(groupCount * 8 + 14);
				triList.Add(groupCount * 8 + 12);

				triList.Add(groupCount * 8 + 5);
				triList.Add(groupCount * 8 + 13);
				triList.Add(groupCount * 8 + 7);
				triList.Add(groupCount * 8 + 13);
				triList.Add(groupCount * 8 + 15);
				triList.Add(groupCount * 8 + 7);
			}

			col0VertList.Add(v0);
			col0VertList.Add(v2);
			col0VertList.Add(v0 - roadThickness * orient);
			col0VertList.Add(v2 - roadThickness * orient);

			if (groupCount >= 0)
			{
				col0TriList.Add(groupCount * 4 + 0);
				col0TriList.Add(groupCount * 4 + 1);
				col0TriList.Add(groupCount * 4 + 4);
				col0TriList.Add(groupCount * 4 + 1);
				col0TriList.Add(groupCount * 4 + 5);
				col0TriList.Add(groupCount * 4 + 4);

				col0TriList.Add(groupCount * 4 + 0);
				col0TriList.Add(groupCount * 4 + 2);
				col0TriList.Add(groupCount * 4 + 4);
				col0TriList.Add(groupCount * 4 + 2);
				col0TriList.Add(groupCount * 4 + 6);
				col0TriList.Add(groupCount * 4 + 4);

				col0TriList.Add(groupCount * 4 + 2);
				col0TriList.Add(groupCount * 4 + 3);
				col0TriList.Add(groupCount * 4 + 6);
				col0TriList.Add(groupCount * 4 + 3);
				col0TriList.Add(groupCount * 4 + 7);
				col0TriList.Add(groupCount * 4 + 6);

				col0TriList.Add(groupCount * 4 + 1);
				col0TriList.Add(groupCount * 4 + 5);
				col0TriList.Add(groupCount * 4 + 3);
				col0TriList.Add(groupCount * 4 + 5);
				col0TriList.Add(groupCount * 4 + 7);
				col0TriList.Add(groupCount * 4 + 3);
			}

			col1VertList.Add(v1);
			col1VertList.Add(v3);
			col1VertList.Add(v1 + roadThickness * orient);
			col1VertList.Add(v3 + roadThickness * orient);

			if (groupCount >= 0)
			{
				col1TriList.Add(groupCount * 4 + 0);
				col1TriList.Add(groupCount * 4 + 1);
				col1TriList.Add(groupCount * 4 + 4);
				col1TriList.Add(groupCount * 4 + 1);
				col1TriList.Add(groupCount * 4 + 5);
				col1TriList.Add(groupCount * 4 + 4);

				col1TriList.Add(groupCount * 4 + 0);
				col1TriList.Add(groupCount * 4 + 2);
				col1TriList.Add(groupCount * 4 + 4);
				col1TriList.Add(groupCount * 4 + 2);
				col1TriList.Add(groupCount * 4 + 6);
				col1TriList.Add(groupCount * 4 + 4);

				col1TriList.Add(groupCount * 4 + 2);
				col1TriList.Add(groupCount * 4 + 3);
				col1TriList.Add(groupCount * 4 + 6);
				col1TriList.Add(groupCount * 4 + 3);
				col1TriList.Add(groupCount * 4 + 7);
				col1TriList.Add(groupCount * 4 + 6);

				col1TriList.Add(groupCount * 4 + 1);
				col1TriList.Add(groupCount * 4 + 5);
				col1TriList.Add(groupCount * 4 + 3);
				col1TriList.Add(groupCount * 4 + 5);
				col1TriList.Add(groupCount * 4 + 7);
				col1TriList.Add(groupCount * 4 + 3);
			}

			groupCount ++;
		});
		_center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
		_mesh.triangles = new int[0];
		_mesh.vertices = vertList.ToArray();
		_mesh.triangles = triList.ToArray();
		_colliderMesh0.triangles = new int[0];
		_colliderMesh1.triangles = new int[0];
		_colliderMesh0.vertices = col0VertList.ToArray();
		_colliderMesh1.vertices = col1VertList.ToArray();
		_colliderMesh0.triangles = col0TriList.ToArray();
		_colliderMesh1.triangles = col1TriList.ToArray();
		_mesh.RecalculateNormals();
		_colliderMesh0.RecalculateNormals();
		_colliderMesh1.RecalculateNormals();
		_meshCol0.sharedMesh = _colliderMesh0;
		_meshCol1.sharedMesh = _colliderMesh1;

		if (onEnd != null)
			onEnd(endX, endY, endGA);
	}

	public Vector3 GetBoundsCenter()
	{
		return _center;
	}
}
