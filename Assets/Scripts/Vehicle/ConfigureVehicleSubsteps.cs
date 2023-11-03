using UnityEngine;

namespace Vehicle
{
	public class ConfigureVehicleSubsteps : MonoBehaviour {

		[SerializeField]
		private float criticalSpeed;
	
		[SerializeField]
		private int stepsBelow;
	
		[SerializeField]
		private int stepsAbove;

		private WheelCollider wheelCollider;

		// Use this for initialization
		void Start () 
		{
			wheelCollider = GetComponentInChildren<WheelCollider>();
			wheelCollider.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
		}
	}
}
