using UnityEngine;

namespace Vehicle
{
	public class VehicleInputController : MonoBehaviour
	{
		public float Horizontal { get; set; }

		public float Forward{ get; set; }

		public float Backward{ get; set; }
		
		void Update ()
		{
			Horizontal = Input.GetAxis ("Horizontal");
			Forward = Mathf.Max(Input.GetAxis("Vertical"), 0);
			Backward = Mathf.Min(Input.GetAxis("Vertical"), 0);
		}
	}
}
