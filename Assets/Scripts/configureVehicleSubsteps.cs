using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class configureVehicleSubsteps : MonoBehaviour {

	public float criticalSpeed;
	public int stepsBelow;
	public int stepsAbove;

	private WheelCollider wc;

	// Use this for initialization
	void Start () {
		wc = GetComponentInChildren<WheelCollider> ();
	}
	
	// Update is called once per frame
	void Update () {
		wc.ConfigureVehicleSubsteps (criticalSpeed, stepsBelow, stepsAbove);
	}
}
