using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDestroy : MonoBehaviour
{
	private carController car;
	private Vector3 vecToObject = Vector3.zero;

	void Start ()
	{	
		car = FindObjectOfType<carController> ();
	}

	void Update ()
	{
		Vector3 vecToObj = transform.position - car.transform.position;
		if (Vector3.Angle (car.transform.forward, vecToObj) > 90 && Vector3.Distance (transform.position, car.transform.position) > 100)
			GameObject.Destroy (gameObject);
	}

	void OnCollisonEnter (Collision col)
	{
		if (col.collider.gameObject.tag == "Player")
			Destroy (this.gameObject);
	}
}