using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NitrousBarScript : MonoBehaviour
{
	public carController car;

	private Image nitrousBar;
	// Use this for initialization
	void Awake ()
	{
		nitrousBar = GetComponent<Image> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		nitrousBar.fillAmount = car.boostAmt / 100;
	}
}
