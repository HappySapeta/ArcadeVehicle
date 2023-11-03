using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
	public GameObject obstacle;
	public carController car;

	public int spawnAmount;
	public float spawnFrameTrigger;
	public float spawnRandomness;
	public float spawnDistance;
	public float speedBasedSpawnFrequency;

	private DataRecorder tdr;
	private Vector3 prevSpawnPoint;
	private bool first = true;

	void Start ()
	{
		tdr = car.GetComponent<DataRecorder> ();
		prevSpawnPoint = car.transform.position;
	}

	Vector3 GetRandomPos ()
	{
		Vector3 pos = car.transform.position + car.transform.forward * spawnDistance * (1 + car.currSpeed * speedBasedSpawnFrequency);
		tdr.obstacleSpawnLocation = pos;
		prevSpawnPoint = pos + car.transform.right * spawnRandomness;
		float xShift = Random.Range (-spawnRandomness, spawnRandomness);

		pos = pos + car.transform.right * xShift;

		return pos;
	}

	bool isBehind (Vector3 pos)
	{
		return (Vector3.Angle (pos - car.transform.position, car.transform.forward) > 70);
	}

	void Update ()
	{
		if (Time.frameCount % spawnFrameTrigger == 0 && isBehind (prevSpawnPoint)) {
			for (int i = 0; i < spawnAmount; i++) {
				GameObject.Instantiate (obstacle, GetRandomPos (), car.transform.rotation);
			}
		}
	}
}