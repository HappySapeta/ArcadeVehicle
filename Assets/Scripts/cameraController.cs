using UnityEngine;
using System.Collections;

public class cameraController : MonoBehaviour
{
	[Range (0, 1)]public float bias = 0.96f;
	public float height;
	public float dist;
	public float shift;
	public float lookUp;
	public Transform target;

	// Use this for initialization


	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		Vector3 newPos = target.position - (target.forward * dist) + (target.up * height) + (target.right * shift);
		Vector3 newFwd = target.forward + Vector3.up * lookUp;
		transform.forward = newFwd;
		transform.position = newPos * (1 - bias) + transform.position * bias;


	}
}
