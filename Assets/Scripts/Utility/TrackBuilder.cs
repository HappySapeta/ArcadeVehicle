using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrackBuilder : MonoBehaviour
{
    [Range(1, 100)] public int divisions = 1;
    public bool showLine = false;
    public bool showPoints = false;
    public Color lineColor = Color.blue;
    public Color pointColor = Color.green;
    public bool reverseTrack;

    private Transform track;
    private Vector3 p0, p1, p2, p3;
    public Vector3[] newWaypoints { get; set; }
    private float[] distances;
    private int i = 0, k = 0, index = 0, x = 0, carPositionIndex;
    private float t = 0;
    private int trackSize;
    private int prevSize;
    private int prevK;
    private Transform[] trackChildren;

    private void Start()
    {
        BuildTrack();
    }

    void OnDrawGizmos()
    {
        BuildTrack();

        for (int i = 0; i < newWaypoints.Length; i++)
        {
            if (showLine)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(newWaypoints[i], newWaypoints[(i + 1) % newWaypoints.Length]);
            }
            if (showPoints)
            {
                Gizmos.color = pointColor;
                Gizmos.DrawWireSphere(newWaypoints[i], 0.05f);
            }

            Vector3 lookPos = transform.GetChild((i / divisions + 1) % track.childCount).position;
            transform.GetChild(i / divisions).LookAt(lookPos);
        }
    }

    public int GetPositionOnTrack(GameObject car)
    {
        float min = float.MaxValue;
        carPositionIndex = 0;
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = Vector3.Distance(car.transform.position, newWaypoints[i]);
            if (min > distances[i])
            {
                min = distances[i];
                carPositionIndex = i;
            }
        }

        return carPositionIndex;
    }

    Vector3 CatMullRom(float t)
    {
        return 0.5f * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t);
    }

    bool ChangeInLength()
    {
        if (prevSize == trackSize * divisions)
            return false;
        else
        {
            prevSize = trackSize * divisions;
            return true;
        }
    }

    public void BuildTrack()
    {
        track = this.transform;
        trackSize = track.childCount;

        trackChildren = new Transform[trackSize];

        for (int i = 0; i < trackSize; i++)
        {
            int id = reverseTrack ? trackSize - i - 1 : i;
            trackChildren[id] = track.GetChild(i);
        }

        if (ChangeInLength())
        {
            newWaypoints = new Vector3[trackSize * divisions];
            distances = new float[newWaypoints.Length];
        }


        if (i > trackSize - 1)
            i = 0;

        p0 = trackChildren[i % trackSize].position;
        p1 = trackChildren[(i + 1) % trackSize].position;
        p2 = trackChildren[(i + 2) % trackSize].position;
        p3 = trackChildren[(i + 3) % trackSize].position;

        k = 0;
        t = 0;
        while (k < divisions)
        {
            index = i * divisions + k;

            newWaypoints[index] = CatMullRom(t);

            t = t + (1 / (float)divisions);
            k++;
        }
        i = (i + 1) % trackSize;
    }

    public float GetTrackProgress(GameObject car)
    {
        int pos_index = GetPositionOnTrack(car);

        return (pos_index / (float)newWaypoints.Length) * 100;
    }
}