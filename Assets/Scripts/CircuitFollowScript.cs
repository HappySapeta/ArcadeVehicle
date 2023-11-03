using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CircuitFollowScript : MonoBehaviour
{
    public carController car;
    public TrackBuilder trackBuilder;
    public int lookAheadOffset;
    public bool gizmo;

    public float offset;
    public float speedFactor, distanceFactor, minSpeed;
    public float maxSpeed = 10;

    public float curve { get; set; }

    private Rigidbody carRigidbody;
    private int finalIndex, carPositionIndex;
    private Vector3 finalPosition;
    private Vector3 lookAhead;

    void Awake()
    {
        carRigidbody = car.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MoveTarget();
    }

    void MoveTarget()
    {
        float newFinalIndex = 0;
        carPositionIndex = trackBuilder.GetPositionOnTrack(car.gameObject);

        newFinalIndex = carPositionIndex + offset * Mathf.Clamp(speedFactor * (car.currSpeed / car.topSpeed), 1, 10);

        finalIndex = (int)newFinalIndex % trackBuilder.newWaypoints.Length;
        finalPosition = trackBuilder.newWaypoints[finalIndex];

        Vector3 nextPosition = trackBuilder.newWaypoints[(int)(newFinalIndex + 1) % trackBuilder.newWaypoints.Length];

        float delta = Mathf.Clamp(speedFactor * car.currSpeed + distanceFactor * (1 / Vector3.Distance(transform.position, car.transform.position)), minSpeed, maxSpeed);

        transform.position = Vector3.MoveTowards(transform.position, finalPosition, delta);
        transform.LookAt(nextPosition);

        lookAhead = trackBuilder.newWaypoints[(finalIndex + lookAheadOffset) % trackBuilder.newWaypoints.Length];
        curve = Vector3.Angle(car.transform.forward, (lookAhead - finalPosition).normalized);
    }

    public int GetTargetPositionIndex()
    {
        return finalIndex;
    }

    private void OnDrawGizmos()
    {
        if (gizmo && Application.isPlaying)
        {
            Gizmos.DrawWireSphere(lookAhead, 0.5f);
            Gizmos.DrawWireSphere(finalPosition, 0.5f);
            Gizmos.DrawLine(finalPosition, lookAhead);
        }
    }
}
