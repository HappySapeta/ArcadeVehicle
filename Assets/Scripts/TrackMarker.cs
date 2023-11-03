using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(TrackBuilder))]
public class TrackMarker : MonoBehaviour
{

    public GameObject markerObject;
    public bool buildMarkers;

    private LineRenderer line_renderer;
    private TrackBuilder track;

    private Vector3[] trackPoints;
    private bool marked, init;

    private void Start()
    {
        track = GetComponent<TrackBuilder>();
    }

    private void Update()
    {
        if (!marked && buildMarkers)
        {
            trackPoints = track.newWaypoints;
            for (int i = 0; i < trackPoints.Length; i++)
            {
                GameObject m = Instantiate(markerObject, trackPoints[i], Quaternion.identity);
                m.hideFlags = HideFlags.HideInHierarchy;
            }
            marked = true;
            buildMarkers = false;
        }
    }
}
