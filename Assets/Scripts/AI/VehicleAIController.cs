using AI;
using UnityEngine;
using Utility;
using Vehicle;

public class VehicleAIController : MonoBehaviour
{
    public enum controlFrom
    {
        cnn, 
        followTarget
    }

    public controlFrom control;
    public Transform target;
    public CircuitFollowScript cfs;
    public Transform track;

    public float steerSensitivity;
    public float brakeSensitivty;
    public float throttleOffset;
    public float obstacleAvoidanceSteerWeight, obstacleAvoidanceBrakeWeight;

    private float directionShift;
    private float distanceToTarget;
    private float distanceToCorner, velocityToCorner;
    public float cornerAngle { get; set; }
    private float factor;
    public float angleToTarget { get; set; }
    private float headingShift;
    private Transform[] trackPoints;
    private VehicleInputController _vehicleInput;
    private VehicleMovement car;
    private Rigidbody carRigidbody;
    public TrackBuilder trackBuilder { get; set; }

    [HideInInspector] public float throttle, brake, steer;
    
    void Awake()
    {
        _vehicleInput = GetComponent<VehicleInputController>();
        trackPoints = new Transform[track.childCount];

        for (int i = 0; i < trackPoints.Length; i++)
        {
            trackPoints[i] = track.GetChild(i);
        }
        car = GetComponent<VehicleMovement>();
        carRigidbody = GetComponent<Rigidbody>();
        trackBuilder = track.GetComponent<TrackBuilder>();
    }

    void Update()
    {
        SetParameters();
        SetSteerInput();
        SetThrottleBrakeInput();

        _vehicleInput.Forward = throttle;
        _vehicleInput.Backward = brake;
        _vehicleInput.Horizontal = steer;
    }

    void SetThrottleBrakeInput()
    {
        float accel = throttleOffset - (factor + 1 / distanceToTarget) * brakeSensitivty;
        accel = Mathf.Clamp(accel, -1, 1);

        throttle = brake = 0;

        if (accel > 0)
        {
            throttle = accel;
        }
        else if (accel == 0)
        {
            throttle = brake = 0;
        }
        else
            brake = accel;
    }

    void SetSteerInput()
    {
        if(control == controlFrom.followTarget)
        {
            float steerValue = steerSensitivity * angleToTarget * Mathf.Sign(car.currSpeed) + obstacleAvoidanceSteerWeight;
            steer = Mathf.Clamp(steerValue, -1, 1);
        }
    }

    void SetParameters()
    {
        Vector3 localTarget = transform.InverseTransformPoint(target.position);
        Transform nextWp = track.GetChild(((trackBuilder.GetPositionOnTrack(gameObject) + 30) / trackBuilder.divisions + 2) % track.childCount);

        angleToTarget = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        distanceToTarget = Vector3.Distance(transform.position, target.position);

        Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 flatHeading = new Vector3(nextWp.forward.x, 0, nextWp.forward.z);

        cornerAngle = Vector3.Angle(flatForward, flatHeading);
        distanceToCorner = Vector3.Distance(transform.position, nextWp.position);
        velocityToCorner = Mathf.Abs(Vector3.Dot(carRigidbody.velocity, nextWp.position - transform.position));

        factor = (velocityToCorner * cornerAngle / (distanceToCorner * distanceToCorner));
    }
}