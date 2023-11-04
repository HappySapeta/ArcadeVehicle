using UnityEngine;

namespace Camera
{
	public class CameraController : MonoBehaviour
	{
		[Range (0, 1), SerializeField]
		private float bias = 0.96f;
	
		[SerializeField]
		private float height;
	
		[SerializeField]
		private float dist;
	
		[SerializeField]
		private float shift;
	
		[SerializeField]
		private float lookUp;
	
		[SerializeField]
		private Transform target;
	
		void FixedUpdate ()
		{
			Vector3 newPos = target.position - (target.forward * dist) + (target.up * height) + (target.right * shift);
			Vector3 newFwd = target.forward + Vector3.up * lookUp;
			transform.forward = newFwd;
			transform.position = newPos * (1 - bias) + transform.position * bias;
		}
	}
}
