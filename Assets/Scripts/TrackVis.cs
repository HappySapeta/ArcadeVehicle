using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackVis : MonoBehaviour
{

	public Color lineColor;

	public bool drawLine;

	void Start ()
	{
		
	}

	void Update ()
	{
		
	}

	void OnDrawGizmos ()
	{
		for (int i = 0; i < transform.childCount - 1; i++) {
			Transform point1 = transform.GetChild (i);
			Transform point2 = transform.GetChild (i + 1);
			if (drawLine)
				Debug.DrawLine (point1.position, point2.position, lineColor);

			point1.LookAt (point2);
		}
	}
}
