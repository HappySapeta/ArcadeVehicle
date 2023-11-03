using UnityEngine;
using System.IO;
using System;

public class DataRecorder : MonoBehaviour
{
	public Transform sensor;

	public float maxSensorRange, sensorRotationAmount, dataCaptureFrequency;
	public bool writeCheck;
	public bool captureZeroes;

	public int size { get; private set; }

	private InputManager inputManager;
	private carController cc;
	private float theta, f;
	private Transform sensorLocal, dirLeft, dirLeftReset;

	public double[] hValues { get; private set; }

	private String writeDirectory = "G:\\Work\\Unity\\Unity Projects\\carSimulator\\Assets\\CAS_trainingData\\TrainingData";
	private StreamWriter writer;
	private int writeCount;

	public float avgObstacleDensity;

	public Vector3 obstacleSpawnLocation{ get; set; }

	void Awake ()
	{
		sensorLocal = sensor.GetChild (0);
		dirLeft = sensor.GetChild (1);
		dirLeftReset = sensor.GetChild (2);

		cc = GetComponent<carController> ();
		inputManager = GetComponent<InputManager> ();

		theta = Vector3.Angle (dirLeftReset.position - sensorLocal.position, sensorLocal.forward);

		size = (int)(2 * theta / sensorRotationAmount + 1);
		hValues = new double[size];
	}

	void Update ()
	{
		StoreCurrentScenario ();
		if (writeCheck)
			WriteDataToFile ();
	}

	float CalculateAvoidanceFactor (RaycastHit x)
	{
		float distance = Vector3.Distance (sensorLocal.position, x.point);
		Vector3 hitDirection = (x.point - sensorLocal.position).normalized;
		float angle = Vector3.SignedAngle (sensorLocal.forward.normalized, hitDirection, sensorLocal.up);
		return 1000 / (angle * angle * distance);
	}

	public double[] StoreCurrentScenario ()
	{
		f = 0;
		int i = 0;
		float rotation = 0;
		dirLeft.LookAt (dirLeftReset);
		float x = -theta;
		avgObstacleDensity = 0;
		while (rotation < 2 * theta) {
			Vector3 dir = dirLeft.forward;
			RaycastHit hit;
			if (Physics.Raycast (dirLeft.position, dirLeft.forward, out hit, maxSensorRange) && hit.transform.tag == "CAS_obstacle") {
				//Debug.DrawLine (dirLeft.position, hit.point, Color.red);
				f = CalculateAvoidanceFactor (hit);
				hValues [i++] = f;
			} else {
				hValues [i++] = 0;
				//Debug.DrawLine (dirLeft.position, dirLeft.position + dirLeft.forward.normalized * 100, Color.green);
			}
			avgObstacleDensity += Mathf.Abs ((float)hValues [i - 1]);
			dirLeft.Rotate (dirLeft.up, sensorRotationAmount);
			rotation = rotation + sensorRotationAmount;
			x += sensorRotationAmount;
		}
		avgObstacleDensity /= hValues.Length;
		return hValues;
	}

	bool WriteCondition ()
	{
		return (Mathf.Abs (Mathf.Round (cc.steerAngle)) > 0) && (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.D));
	}

	void WriteDataToFile ()
	{
		if (writer == null) {
			writer = new StreamWriter (writeDirectory);
		}
		if (writer != null && writeCount <= 40000 && Time.frameCount % dataCaptureFrequency == 0 && cc.currSpeed > 40) {
			if (this.isActiveAndEnabled && (WriteCondition () || captureZeroes)) {
				
				writer.Write (Mathf.Round (cc.steerAngle));

				for (int i = 0; i < hValues.Length; i++)
					writer.Write ("," + hValues [i]);

				writer.Write ("\n");
				writeCount++;
				Debug.Log ("writing, line : " + writeCount);
			} 
		} else
			Debug.Log ("Not writing, line : " + writeCount);

		if (writeCount > 40000)
			writer.Close ();
	}

	void OnQuitApplication ()
	{
		writer.Close ();
	}
}