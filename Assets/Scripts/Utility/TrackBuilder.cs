using UnityEngine;
using UnityEngine.Serialization;

namespace Utility
{
    public class TrackBuilder : MonoBehaviour
    {
        public Vector3[] newWaypoints { get; set; }

        [Range(1, 100)] public int divisions;

        [FormerlySerializedAs("showLine")] [SerializeField]
        private bool drawDebugLine;

        [FormerlySerializedAs("showPoints")] [SerializeField]
        private bool drawDebugPoints;

        [FormerlySerializedAs("lineColor")] [SerializeField]
        private Color debugLineColor = Color.blue;

        [FormerlySerializedAs("pointColor")] [SerializeField]
        private Color debugPointColor = Color.green;

        [SerializeField] private bool reverseTrack;

        private Transform track;
        private Vector3 p0, p1, p2, p3;
        private float[] distances;
        private int currentTrackIndex, k, index, carPositionIndex;
        private float t;
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
                if (drawDebugLine)
                {
                    Gizmos.color = debugLineColor;
                    Gizmos.DrawLine(newWaypoints[i], newWaypoints[(i + 1) % newWaypoints.Length]);
                }

                if (drawDebugPoints)
                {
                    Gizmos.color = debugPointColor;
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

        Vector3 CatMullRom(float step)
        {
            Vector3 intermediatePoint = 0.5f * ((2 * p1) +
                                        (-p0 + p2) * step +
                                        (2 * p0 - 5 * p1 + 4 * p2 - p3) * step * step +
                                        (-p0 + 3 * p1 - 3 * p2 + p3) * step * step * step);
            return intermediatePoint;
        }

        bool ChangeInLength()
        {
            if (prevSize == trackSize * divisions)
            {
                return false;
            }
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


            if (currentTrackIndex > trackSize - 1)
            {
                currentTrackIndex = 0;
            }

            p0 = trackChildren[currentTrackIndex % trackSize].position;
            p1 = trackChildren[(currentTrackIndex + 1) % trackSize].position;
            p2 = trackChildren[(currentTrackIndex + 2) % trackSize].position;
            p3 = trackChildren[(currentTrackIndex + 3) % trackSize].position;

            k = 0;
            t = 0;
            while (k < divisions)
            {
                index = currentTrackIndex * divisions + k;
                newWaypoints[index] = CatMullRom(t);
                t = t + (1 / (float)divisions);
                k++;
            }

            currentTrackIndex = (currentTrackIndex + 1) % trackSize;
        }

        public float GetTrackProgress(GameObject car)
        {
            int pos_index = GetPositionOnTrack(car);
            return (pos_index / (float)newWaypoints.Length) * 100;
        }
    }
}