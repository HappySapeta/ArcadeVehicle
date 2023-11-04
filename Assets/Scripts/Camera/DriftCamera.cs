using System;
using UnityEngine;
using Vehicle;

public class DriftCamera : MonoBehaviour
{
	[Serializable]
	public class AdvancedOptions
	{
		public bool updateCameraInUpdate;
		public bool updateCameraInFixedUpdate = true;
		public bool updateCameraInLateUpdate;
		public KeyCode switchViewKey = KeyCode.Space;
	}

	public VehicleMovement car;
	public float speedFactor;
	public float basePositionValue;

	public float smoothing = 6f;
	public Transform lookAtTarget;
	public Transform positionTarget;
	public Transform sideView;
	public AdvancedOptions advancedOptions;

	bool m_ShowingSideView;

	private void FixedUpdate ()
	{
		if (advancedOptions.updateCameraInFixedUpdate)
			UpdateCamera ();
	}

	private void Update ()
	{
		if (Input.GetKeyDown (advancedOptions.switchViewKey))
			m_ShowingSideView = !m_ShowingSideView;

		if (advancedOptions.updateCameraInUpdate)
			UpdateCamera ();
	}

	private void LateUpdate ()
	{
		if (advancedOptions.updateCameraInLateUpdate)
			UpdateCamera ();
	}

	private void UpdateCamera ()
	{
		Vector3 newPos = positionTarget.localPosition;

		newPos.z = basePositionValue + (car.currentSpeed / car.topSpeed) * speedFactor;
		positionTarget.localPosition = newPos;
		if (m_ShowingSideView) {
			transform.position = sideView.position;
			transform.rotation = sideView.rotation;
		} else {
			transform.position = Vector3.Lerp (transform.position, positionTarget.position, Time.deltaTime * smoothing);
			transform.LookAt (lookAtTarget);
		}
	}
}
