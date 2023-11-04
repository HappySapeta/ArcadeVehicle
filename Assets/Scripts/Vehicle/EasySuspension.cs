// Adapted from - https://github.com/Unity-Technologies/VehicleTools/blob/master/Assets/Scripts/EasySuspension.cs
// Credits - Unity-Technologies

using UnityEngine;

namespace Vehicle
{
	[ExecuteInEditMode]
	public class EasySuspension : MonoBehaviour
	{
		[Range (0.1f, 20f)]
		[Tooltip ("Natural frequency of the suspension springs. Describes bounciness of the suspension.")]
		public float naturalFrequency = 10;

		[Range (0f, 3f)]
		[Tooltip ("Damping ratio of the suspension springs. Describes how fast the spring returns back after a bounce. ")]
		public float dampingRatio = 0.8f;

		[Range (-1f, 1f)]
		[Tooltip ("The distance along the Y axis the suspension forces application point is offset below the center of mass")]
		public float forceShift = 0.03f;

		[Tooltip ("Adjust the length of the suspension springs according to the natural frequency and damping ratio. When off, can cause unrealistic suspension bounce.")]
		public bool setSuspensionDistance = true;

		Rigidbody m_Rigidbody;
		private WheelCollider[] m_WheelColliders;

		void Start ()
		{
			m_Rigidbody = GetComponent<Rigidbody> ();
			m_WheelColliders = GetComponentsInChildren<WheelCollider>();
		}

		void Update ()
		{
			// Work out the stiffness and damper parameters based on the better spring model.
			foreach (WheelCollider wheel in m_WheelColliders) 
			{
				JointSpring spring = wheel.suspensionSpring;

				float sqrtWcSprungMass = Mathf.Sqrt (wheel.sprungMass);
				spring.spring = sqrtWcSprungMass * naturalFrequency * sqrtWcSprungMass * naturalFrequency;
				spring.damper = 2f * dampingRatio * Mathf.Sqrt (spring.spring * wheel.sprungMass);

				wheel.suspensionSpring = spring;

				Vector3 wheelRelativeBody = transform.InverseTransformPoint (wheel.transform.position);
				float distance = m_Rigidbody.centerOfMass.y - wheelRelativeBody.y + wheel.radius;

				wheel.forceAppPointDistance = distance - forceShift;

				// Make sure the spring force at maximum droop is exactly zero
				if (spring.targetPosition > 0 && setSuspensionDistance)
				{ 
					wheel.suspensionDistance = wheel.sprungMass * Physics.gravity.magnitude / (spring.targetPosition * spring.spring);
				}
			}
		}
	}
}
