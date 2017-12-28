using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPointBuilder
{
	public struct RoadPoint
	{
		public float x;
		public float y;
		public float na;

		public RoadPoint(float x, float y, float na)
		{
			this.x = x;
			this.y = y;
			this.na = na;
		}
	}

	private class CursorLine
	{
		private float pivotX;
		private float pivotY;
		private float gradientAngle;
		private float cursor0;
		private float cursor1;

		public float GetPivotX()
		{
			return pivotX;
		}

		public float GetPivotY()
		{
			return pivotY;
		}

		public float GetGradientAngle()
		{
			return gradientAngle;
		}

		public float GetCursor0()
		{
			return cursor0;
		}

		public float GetCursor1()
		{
			return cursor1;
		}

		public CursorLine(float pX, float pY, float g, float c0, float c1)
		{
			pivotX = pX;
			pivotY = pY;
			gradientAngle = g;
			cursor0 = c0;
			cursor1 = c1;
		}

		private void GetPoint(float t, out float ox, out float oy)
		{
			ox = pivotX + Mathf.Cos(gradientAngle) * t;
			oy = pivotY + Mathf.Sin(gradientAngle) * t;
		}

		public RoadPoint roadPoint0
		{
			get
			{
				float x = 0.0f, y = 0.0f, na = 0.0f;
				GetPoint(cursor0, out x, out y);
				if (cursor0 <= cursor1)
					na = gradientAngle;
				else
					na = gradientAngle + Mathf.PI;
				return new RoadPoint(x, y, na);
			}
		}
		public RoadPoint roadPoint1
		{
			get
			{
				float x = 0.0f, y = 0.0f, na = 0.0f;
				GetPoint(cursor1, out x, out y);
				if (cursor0 > cursor1)
					na = gradientAngle;
				else
					na = gradientAngle + Mathf.PI;
				return new RoadPoint(x, y, na);
			}
		}

		public void ChangeAngle(float deltaAngle)
		{
			gradientAngle += deltaAngle;
		}

		public void ChangeAngleByDistance(float distance)
		{
			float r = (cursor0 + cursor1) * 0.5f;
			float a = Mathf.Asin(distance * 0.5f / r) * 2;
			ChangeAngle(a);
		}

		public void ChangePivot(float t)
		{
			float x = 0.0f, y = 0.0f;
			GetPoint(t, out x, out y);
			pivotX = x;
			pivotY = y;
			cursor0 -= t;
			cursor1 -= t;
		}

		public void GetMiddlePoint(out float x, out float y)
		{
			GetPoint((cursor0 + cursor1) / 2, out x, out y);
		}
	}

	private CursorLine _cursorLine = null;
	private float stepDistance = 0.1f;
	private List<RoadPoint> roadSide0 = null;
	private List<RoadPoint> roadSide1 = null;

	public RoadPointBuilder(float px, float py, float na, float c0, float c1, float stepDist)
	{
		_cursorLine = new CursorLine(px, py, na, c0, c1);
		stepDistance = stepDist;
		roadSide0 = new List<RoadPoint>();
		roadSide1 = new List<RoadPoint>();
	}

	public void BuildDistance(float distance)
	{
		float restDist = distance;
		while(Mathf.Abs(restDist) > float.Epsilon)
		{
			float targetDistance = Mathf.Clamp(restDist, -stepDistance, stepDistance);
			restDist -= targetDistance;
			roadSide0.Add(_cursorLine.roadPoint0);
			roadSide1.Add(_cursorLine.roadPoint1);
			_cursorLine.ChangeAngleByDistance(targetDistance);
		}
	}

	public void ChangePivot(float t)
	{
		_cursorLine.ChangePivot(t);
	}

	public void ForEachRoadPoint(System.Action<RoadPoint, RoadPoint> cb)
	{
		int length = Mathf.Min(roadSide0.Count, roadSide1.Count);
		for (int i = 0; i < length; i++)
			cb(roadSide0[i], roadSide1[i]);
	}

	public void QueryCursorState(System.Action<float, float, float, float, float> cb)
	{
		cb(_cursorLine.GetPivotX(), _cursorLine.GetPivotY(), _cursorLine.GetGradientAngle(), _cursorLine.GetCursor0(), _cursorLine.GetCursor1());
	}

	public int GetBlockCount()
	{
		int pointCount = Mathf.Min(roadSide0.Count, roadSide1.Count);
		return pointCount / 2 - 1;
	}

	public int GetInsectBlock(float x, float y)
	{
		for (int i = 0, n = Mathf.Min(roadSide0.Count, roadSide1.Count) - 1; i < n; i++)
		{
			float x0 = roadSide0[i + 0].x, y0 = roadSide0[i + 0].y;
			float x1 = roadSide1[i + 0].x, y1 = roadSide1[i + 0].y;
			float x2 = roadSide1[i + 1].x, y2 = roadSide1[i + 1].y;
			float x3 = roadSide0[i + 1].x, y3 = roadSide0[i + 1].y;

			float k0 = (x0 - x) * (y1 - y0) - (x1 - x0) * (y0 - y);
			float k1 = (x1 - x) * (y2 - y1) - (x2 - x1) * (y1 - y);
			float k2 = (x2 - x) * (y3 - y2) - (x3 - x2) * (y2 - y);
			float k3 = (x3 - x) * (y0 - y3) - (x0 - x3) * (y3 - y);

			if (
				(k0 >= 0.0f && k1 >= 0.0f && k2 >= 0.0f && k3 >= 0.0f) ||
				(k0 <= 0.0f && k1 <= 0.0f && k2 <= 0.0f && k3 <= 0.0f)
			)
				return i / 2;
		}
		return -1;
	}

	public void GetCursorMiddlePoint(out float x, out float y)
	{
		_cursorLine.GetMiddlePoint(out x, out y);
	}
}
