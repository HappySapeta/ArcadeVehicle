using Vehicle;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
	public class TachometerDisplay : MonoBehaviour
	{
		[FormerlySerializedAs("cc"), SerializeField] 
		private VehicleMovement vehicle;

		[SerializeField] 
		private float maxInput;
	
		[SerializeField]
		private float minInput;
	
		[FormerlySerializedAs("baseCorrection")] [FormerlySerializedAs("correction")] [SerializeField]
		private float angleOffset;
	
		[FormerlySerializedAs("multiplier")] [SerializeField]
		private float angleMultiplier;
	
		[SerializeField]
		private float rotationSpeed;
	
		private float input;
	
		void Update ()
		{
			input = Mathf.Abs (vehicle.currentEngineRPM);
			input = Mathf.Clamp (input, minInput, vehicle.maxGearChangeRPM);

			float angle = 0;
			angle = Mathf.SmoothStep (angle, Mathf.Asin (input / maxInput) * angleMultiplier, rotationSpeed);

			float valueCorrection = (vehicle.currentGearNum == 1 || vehicle.currentGearNum == -1) ? 25 : angleOffset;
			angle += valueCorrection;

			Quaternion newRot = Quaternion.Euler (0, 0, angle);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, newRot, rotationSpeed);
		}
	}
}
