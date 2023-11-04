// Adapted from - https://github.com/peko/Unity-Car/blob/master/Project/Assets/AntiRollBar.cs.
// Credits - Vladimir Seregin aka peko.

using UnityEngine;

namespace Vehicle
{
	public class AntiRollBar : MonoBehaviour {

		[SerializeField]
		private WheelCollider WheelL;
	
		[SerializeField]
		private WheelCollider WheelR;
	
		[SerializeField]
		private float AntiRoll = 5000.0f;

		private Rigidbody carRigidbody;

		void Start()
		{
			carRigidbody = GetComponent<Rigidbody> ();
		}

		void FixedUpdate ()
		{
			WheelHit hit;
			float travelL = 1.0f;
			float travelR = 1.0f;

			Vector3 WheelLPosition = WheelL.transform.position;
			Vector3 WheelRPosition = WheelR.transform.position;

			bool groundedL = WheelL.GetGroundHit (out hit);
			if (groundedL) 
			{
				travelL = (-WheelL.transform.InverseTransformPoint (hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
			}

			bool groundedR = WheelR.GetGroundHit (out hit);
			if (groundedR) 
			{
				travelR = (-WheelR.transform.InverseTransformPoint (hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
			}

			float antiRollForce = (travelL - travelR) * AntiRoll;

			if (groundedL)
			{
				carRigidbody.AddForceAtPosition (WheelL.transform.up * -antiRollForce, WheelLPosition);
			}

			if (groundedR)
			{
				carRigidbody.AddForceAtPosition (WheelR.transform.up * antiRollForce, WheelRPosition);
			}
		}
	}
}
