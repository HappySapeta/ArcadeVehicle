using System;
using System.Collections;
using UnityEngine;

namespace Vehicle
{
    [Serializable]
    enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }
    
    [Serializable]
    struct EngineParameters
    {
        public AnimationCurve torqueCurve;
        public float idleRPM;
        public float maxGearChangeRPM;
        public float minGearChangeRPM;
    }

    [Serializable]
    struct DrivetrainParameters
    {
        public DriveType carDriveType;
        public AnimationCurve gearRatios;
        public float[] gearSpeeds;
        public bool validateGearSpeeds;
        public float finalDriveRatio1;
        public float finalDriveRatio2;
        public float differentialTorqueDrop;
    }

    [Serializable]
    struct ArcadeMovementParameters
    {
        public float topSpeed;
        public float accleration;
        public float boostForce;
        public float boostAmt;
        public float brakingPower;
        public float topSpeedDrag;
        public float idleDrag;
        public float runningDrag;
    }

    [Serializable]
    struct ArcadeWheelParameters
    {
        public float forwardFrictionSpeedFactor;
        public float baseFwdExtremum;
        public float baseFwdAsymptote;
        public float baseSideAsymptote;
        public float baseSideExtremum;
        public float driftVelocityFactor;
        public float defaultForwardStiffness;
        public float maxSidewaysStiffness;
        public float maxSidewaysFrictionValue;
    }

    [Serializable]
    struct ArcadeSteeringParameters
    {
        public float turnPower;
        public float revTorquePower;
        public float maxSteerAngle;
        public float steerSensitivity;
        public float speedDependencyFactor;
        public float steerAngleLimitingFactor;
    }

    [Serializable]
    struct WheelSetup
    {
        public Transform[] wheelMesh;
        public WheelCollider[] wheelColliders;
    }
    
    [RequireComponent(typeof(VehicleInputController))]
    public class VehicleMovement : MonoBehaviour
    {
        [SerializeField] private EngineParameters engineParameters;
        [SerializeField] private DrivetrainParameters drivetrainParameters;
        [SerializeField] private ArcadeMovementParameters arcadeMovementParameters;
        [SerializeField] private ArcadeWheelParameters arcadeWheelParameters;
        [SerializeField] private ArcadeSteeringParameters arcadeSteeringParameters;
        [SerializeField] private WheelSetup wheelSetup;
        
        [SerializeField] private float driftPower;
        [SerializeField] private float traction;
        [SerializeField] private float slipLimit;
        [SerializeField] private float criticalDonutSpeed;
        
        public float maxGearChangeRPM => engineParameters.maxGearChangeRPM;
        
        public float topSpeed => arcadeMovementParameters.topSpeed;

        public float boostAmt => arcadeMovementParameters.boostAmt;
        
        private bool _isBoosting;
        public bool isBoosting => _isBoosting;
        
        private float _currentEngineRPM;
        public float currentEngineRPM => _currentEngineRPM;
        
        private int _currentGearNum = 1;
        public int currentGearNum => _currentGearNum;
        
        private float _currSpeed;
        public float currentSpeed => _currSpeed;
        
        private VehicleInputController inputController;
        private Rigidbody _rigidbody;
        
        private float fwdInput, backInput, horizontalInput;
        private float totalTorque;
        private float outputTorque;
        private float currentWheelRpm;
        private float steerAngle;
        private float finalDrive;
        private float currentTorque;
        private float oldRotation;
        private float localSteerHelper;
        private bool drifting;
        private float upClamp;
        private float boostRefillWait = 5;
        private bool isRefilling;
        private bool burnOut;
        private float prevAngularVelocity;
        private float angularAcclY;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            inputController = GetComponent<VehicleInputController>();
        }

        void FixedUpdate()
        {
            fwdInput = inputController.Forward;
            backInput = inputController.Backward;
            horizontalInput = inputController.Horizontal;
            adjustFinalDrive();
            addBoostTorque();
            moveCar();
            steerCar();
            brakeCar();
            animateWheels();
            getCarSpeed();
            calcTurnAngle();
            adjustDrag();
            adjustForwardFriction();
            adjustSidewaysFriction();
            rotationalStabilizer();
            steerHelper();
            tractionControl();
            driftCar();
        }

        void moveCar()
        {
            float leftWheelTorque, rightWheelTorque;
            calcTorque();
            if (drivetrainParameters.carDriveType == DriveType.AllWheelDrive)
            {
                outputTorque = totalTorque / 4;
                leftWheelTorque = outputTorque * (1 - Mathf.Clamp(drivetrainParameters.differentialTorqueDrop * ((steerAngle < 0) ? -steerAngle : 0), 0, 0.9f));
                rightWheelTorque = outputTorque * (1 - Mathf.Clamp(drivetrainParameters.differentialTorqueDrop * ((steerAngle > 0) ? steerAngle : 0), 0, 0.9f));
                wheelSetup.wheelColliders[0].motorTorque = wheelSetup.wheelColliders[2].motorTorque = leftWheelTorque;
                wheelSetup.wheelColliders[1].motorTorque = wheelSetup.wheelColliders[3].motorTorque = rightWheelTorque;
            }
            else if (drivetrainParameters.carDriveType == DriveType.FrontWheelDrive)
            {
                outputTorque = totalTorque / 2;
                for (int i = 0; i < 2; i++)
                {
                    wheelSetup.wheelColliders[i].motorTorque = outputTorque;
                }
            }
            else
            {
                outputTorque = totalTorque / 2;
                for (int i = 2; i < 4; i++)
                {
                    wheelSetup.wheelColliders[i].motorTorque = outputTorque;
                }
            }
        }

        void steerCar()
        {
            float x = horizontalInput * (arcadeSteeringParameters.maxSteerAngle - (_currSpeed / arcadeMovementParameters.topSpeed) * arcadeSteeringParameters.steerAngleLimitingFactor);
            float steerSpeed = arcadeSteeringParameters.steerSensitivity + (_currSpeed / arcadeMovementParameters.topSpeed) * arcadeSteeringParameters.speedDependencyFactor;

            steerAngle = Mathf.SmoothStep(steerAngle, x, steerSpeed);

            wheelSetup.wheelColliders[0].steerAngle = steerAngle;
            wheelSetup.wheelColliders[1].steerAngle = steerAngle;

            if (!isFlying())
            {
                _rigidbody.AddRelativeTorque(transform.up * (arcadeSteeringParameters.turnPower * _currSpeed * horizontalInput));
            }
        }

        void brakeCar()
        {
            int dir = 0;

            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    wheelSetup.wheelColliders[i].brakeTorque = arcadeMovementParameters.brakingPower;
                }
                else
                    wheelSetup.wheelColliders[i].brakeTorque = 0;
            }

            if (backInput < 0 && currentWheelRpm > 0 && _currSpeed > 0)
            {
                dir = 1;
            }
            else if (fwdInput > 0 && currentWheelRpm < 0 && _currSpeed < 0)
            {
                dir = 1;
            }

            wheelSetup.wheelColliders[0].brakeTorque = wheelSetup.wheelColliders[1].brakeTorque = dir * arcadeMovementParameters.brakingPower;

            if ((Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) ||
                (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.UpArrow)) && _currSpeed < 5)
            {
                burnOut = true;
                wheelSetup.wheelColliders[0].brakeTorque = wheelSetup.wheelColliders[1].brakeTorque = 5000;
            }
            else
                burnOut = false;
        }

        void driftCar()
        {
            if (_currSpeed > 0 && Mathf.Abs(horizontalInput) > 0 && (backInput < 0 || Input.GetKey(KeyCode.Space)) &&
                (!isFlying()))
            {
                float localDriftPower = Input.GetKey(KeyCode.Space) ? driftPower : 0.8f * driftPower;
                float torque = Mathf.Clamp(localDriftPower * horizontalInput * _currSpeed, -15000, 15000);
                _rigidbody.AddRelativeTorque(transform.up * torque);
            }
        }

        void adjustFinalDrive()
        {
            if (_currentGearNum == 1 || _currentGearNum == 4 || _currentGearNum == 5)
            {
                finalDrive = drivetrainParameters.finalDriveRatio1;
            }
            else
            {
                finalDrive = drivetrainParameters.finalDriveRatio2;
            }
        }

        void addBoostTorque()
        {
            if (Input.GetKey(KeyCode.LeftShift) && arcadeMovementParameters.boostAmt > 0 && _currSpeed > 0)
            {
                _isBoosting = true;
                isRefilling = false;
                arcadeMovementParameters.boostAmt = Mathf.MoveTowards(arcadeMovementParameters.boostAmt, 0, 0.15f);
            }
            else
                _isBoosting = false;

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                StartCoroutine(boostWaitBeforeRefill());
            }

            if (isRefilling)
            {
                arcadeMovementParameters.boostAmt = Mathf.MoveTowards(arcadeMovementParameters.boostAmt, 100, 0.05f);
            }
        }

        IEnumerator boostWaitBeforeRefill()
        {
            yield return new WaitForSeconds(boostRefillWait);
            isRefilling = true;
        }

        void calcTorque()
        {
            float accleration = (_currentGearNum == 1) ? Mathf.MoveTowards(0, 1 * fwdInput, 0.8f) : arcadeMovementParameters.accleration; // 0.8f = thrAgg????
            float throttle = (_currentGearNum == -1) ? backInput : fwdInput;
            shiftGear();
            getEngineRPM();
            totalTorque = engineParameters.torqueCurve.Evaluate(_currentEngineRPM) * (drivetrainParameters.gearRatios.Evaluate(_currentGearNum)) * finalDrive * throttle *
                          accleration;
            if (_isBoosting)
            {
                totalTorque += arcadeMovementParameters.boostForce;
            }

            if (_currentEngineRPM >= engineParameters.maxGearChangeRPM)
                totalTorque = 0;
            tractionControl();
        }

        void shiftGear()
        {
            if ((_currentGearNum < drivetrainParameters.gearRatios.length - 1 && _currentEngineRPM >= engineParameters.maxGearChangeRPM ||
                 (_currentGearNum == 0 && (fwdInput > 0 || backInput < 0))) && !isFlying() && checkGearSpeed())
            {
                //Debug.Log (_currSpeed);
                _currentGearNum++;
            }

            if (_currentGearNum > 1 && _currentEngineRPM <= engineParameters.minGearChangeRPM)
                _currentGearNum--;
            if (checkStandStill() && backInput < 0)
                _currentGearNum = -1;
            if (_currentGearNum == -1 && checkStandStill() && fwdInput > 0)
                _currentGearNum = 1;
        }

        bool checkGearSpeed()
        {
            if (_currentGearNum != -1)
            {
                if (drivetrainParameters.validateGearSpeeds)
                {
                    return _currSpeed >= drivetrainParameters.gearSpeeds[_currentGearNum - 1];
                }
                else
                    return true;
            }
            else
                return false;
        }

        float idlingRPM()
        {
            return _currentGearNum > 1 ? 0 : engineParameters.idleRPM;
        }

        void getEngineRPM()
        {
            idlingRPM();
            getWheelRPM();
            float velocity = 0.0f;
            _currentEngineRPM = Mathf.SmoothDamp(_currentEngineRPM, idlingRPM() + (Mathf.Abs(currentWheelRpm) * finalDrive * drivetrainParameters.gearRatios.Evaluate(_currentGearNum)), ref velocity, 0.05f);
        }

        void getWheelRPM()
        {
            float sum = 0;
            int c = 0;
            for (int i = 0; i < 4; i++)
            {
                if (wheelSetup.wheelColliders[i].isGrounded)
                {
                    sum += wheelSetup.wheelColliders[i].rpm;
                    c++;
                }
            }

            currentWheelRpm = (c != 0) ? sum / c : 0;
        }

        void getCarSpeed()
        {
            _currSpeed = Vector3.Dot(transform.forward.normalized, _rigidbody.velocity);
            _currSpeed *= 3.6f;
            _currSpeed = Mathf.Round(_currSpeed);
        }

        void animateWheels()
        {
            Vector3 wheelPosition;
            Quaternion wheelRotation;

            for (int i = 0; i < 4; i++)
            {
                wheelSetup.wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
                wheelSetup.wheelMesh[i].position = wheelPosition;
                wheelSetup.wheelMesh[i].rotation = wheelRotation;
            }
        }

        void adjustDrag()
        {
            if (_currSpeed >= arcadeMovementParameters.topSpeed)
                _rigidbody.drag = arcadeMovementParameters.topSpeedDrag;
            else if (outputTorque == 0)
                _rigidbody.drag = arcadeMovementParameters.idleDrag;
            else if (_currSpeed >= 30.0f && _currentGearNum == -1 && currentWheelRpm <= 0) // 30 = maxReverseSpeed
                _rigidbody.drag = 0.1f; // 0.1f = reverseDrag
            else
            {
                _rigidbody.drag = arcadeMovementParameters.runningDrag;
            }
        }

        bool isFlying()
        {
            if (!wheelSetup.wheelColliders[0].isGrounded && !wheelSetup.wheelColliders[1].isGrounded && !wheelSetup.wheelColliders[2].isGrounded &&
                !wheelSetup.wheelColliders[3].isGrounded)
            {
                return true;
            }
            else
                return false;
        }

        bool checkStandStill()
        {
            if (_currSpeed == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void calcTurnAngle()
        {
            Vector3 flatForward = transform.forward;
            flatForward.y = 0;
            if (flatForward.sqrMagnitude > 0)
            {
                flatForward.Normalize();
                transform.InverseTransformDirection(flatForward);
            }
        }

        void adjustForwardFriction()
        {
            WheelFrictionCurve forwardFriction = wheelSetup.wheelColliders[0].forwardFriction;
            forwardFriction.extremumValue = arcadeWheelParameters.baseFwdExtremum + ((_currSpeed <= 0) ? 0 : _currSpeed / arcadeMovementParameters.topSpeed) * arcadeWheelParameters.forwardFrictionSpeedFactor;
            forwardFriction.asymptoteValue = arcadeWheelParameters.baseFwdAsymptote + ((_currSpeed <= 0) ? 0 : _currSpeed / arcadeMovementParameters.topSpeed) * arcadeWheelParameters.forwardFrictionSpeedFactor;

            forwardFriction.extremumValue = Mathf.Clamp(forwardFriction.extremumValue, arcadeWheelParameters.baseFwdExtremum, 5);
            forwardFriction.asymptoteValue = Mathf.Clamp(forwardFriction.asymptoteValue, arcadeWheelParameters.baseFwdAsymptote, 5);

            if (burnOut)
            {
                forwardFriction.extremumValue = 0.1f;
                forwardFriction.asymptoteValue = 0.1f;
            }

            for (int i = 0; i < 4; i++)
            {
                wheelSetup.wheelColliders[i].forwardFriction = forwardFriction;
            }
        }

        void adjustSidewaysFriction()
        {
            upClamp = Mathf.SmoothStep(upClamp, arcadeWheelParameters.maxSidewaysFrictionValue, 0.2f);
            float driftX = Mathf.Abs(transform.InverseTransformVector(_rigidbody.velocity).x);
            float driftFactor = driftX * arcadeWheelParameters.driftVelocityFactor;

            WheelFrictionCurve sidewaysFriction = wheelSetup.wheelColliders[0].sidewaysFriction;

            float x = arcadeWheelParameters.baseSideAsymptote + driftFactor;
            float y = arcadeWheelParameters.baseSideExtremum + driftFactor;

            if (Mathf.Abs(_currSpeed) < criticalDonutSpeed)
            {
                sidewaysFriction.stiffness = 0.8f;
            }
            else
            {
                sidewaysFriction.stiffness = arcadeWheelParameters.maxSidewaysStiffness;
            }

            sidewaysFriction.asymptoteValue = Mathf.Clamp(x, arcadeWheelParameters.baseSideAsymptote, arcadeWheelParameters.maxSidewaysFrictionValue);
            sidewaysFriction.extremumValue = Mathf.Clamp(y, arcadeWheelParameters.baseSideAsymptote, arcadeWheelParameters.maxSidewaysFrictionValue);

            for (int i = 0; i < 4; i++)
            {
                wheelSetup.wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
        }

        void steerHelper()
        {
            foreach (WheelCollider wc in wheelSetup.wheelColliders)
            {
                WheelHit wheelHit;
                wc.GetGroundHit(out wheelHit);
                if (wheelHit.normal == Vector3.zero)
                    return;
            }

            if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10)
            {
                float turnAdjust = (transform.eulerAngles.y - oldRotation);
                Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
                _rigidbody.velocity = velRotation * _rigidbody.velocity;
            }

            oldRotation = transform.eulerAngles.y;
        }

        void adjustTorque(float forwardSlip)
        {
            if (forwardSlip >= slipLimit && currentTorque >= 0)
            {
                currentTorque -= 1000 * traction;
            }
            else
            {
                currentTorque += 1000 * traction;
                if (currentTorque >= totalTorque)
                {
                    currentTorque = totalTorque;
                }
            }
        }

        void rotationalStabilizer()
        {
            calcAngularAccl();

            float reverseTorque = -1 * Mathf.Abs(angularAcclY) * arcadeSteeringParameters.revTorquePower * Mathf.Sign(_rigidbody.angularVelocity.y) * (_currSpeed / arcadeMovementParameters.topSpeed);

            _rigidbody.AddRelativeTorque(transform.up * reverseTorque);
        }

        void calcAngularAccl()
        {
            var angularVelocity = _rigidbody.angularVelocity;
            
            angularAcclY = (prevAngularVelocity - angularVelocity.y) / Time.deltaTime;
            prevAngularVelocity = angularVelocity.y;
        }

        void tractionControl()
        {
            WheelHit wheelHit;

            switch (drivetrainParameters.carDriveType)
            {
                case DriveType.AllWheelDrive:
                {
                    foreach (WheelCollider wc in wheelSetup.wheelColliders)
                    {
                        wc.GetGroundHit(out wheelHit);
                        adjustTorque(wheelHit.forwardSlip);
                    }

                    break;
                }
                case DriveType.RearWheelDrive:
                {
                    wheelSetup.wheelColliders[2].GetGroundHit(out wheelHit);
                    adjustTorque(wheelHit.forwardSlip);
                    wheelSetup.wheelColliders[3].GetGroundHit(out wheelHit);
                    adjustTorque(wheelHit.forwardSlip);

                    break;
                }
                case DriveType.FrontWheelDrive:
                {
                    wheelSetup.wheelColliders[0].GetGroundHit(out wheelHit);
                    adjustTorque(wheelHit.forwardSlip);
                    wheelSetup.wheelColliders[1].GetGroundHit(out wheelHit);
                    adjustTorque(wheelHit.forwardSlip);

                    break;
                }
            }
        }
    }
}
