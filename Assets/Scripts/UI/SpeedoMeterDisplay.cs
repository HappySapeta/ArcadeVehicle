using Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SpeedoMeterDisplay : MonoBehaviour
	{
		[SerializeField] 
		private VehicleMovement vehicle;

		private Text digit0, digit1, digit2, gear;

		// Use this for initialization
		void Awake ()
		{
			digit0 = transform.GetChild (0).GetComponent<Text> ();
			digit1 = transform.GetChild (1).GetComponent<Text> ();
			digit2 = transform.GetChild (2).GetComponent<Text> ();
			gear = transform.GetChild (3).GetComponent<Text> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
			int unitsPlace = Mathf.Abs((int)(vehicle.currentSpeed % 10));
			int tensPlace = Mathf.Abs((int)((vehicle.currentSpeed / 10) % 10));
			int hundredsPlace = Mathf.Abs((int)((vehicle.currentSpeed / 100) % 10));

			digit2.text = unitsPlace.ToString ();
			digit1.text = tensPlace.ToString ();
			digit0.text = hundredsPlace.ToString ();

			switch (vehicle.currentGearNum)
			{
				case -1:
				{
					gear.text = "R";
					break;
				}

				case 0:
				{
					gear.text = "N";
					break;
				}

				case 1:
				{
					gear.text = vehicle.currentGearNum.ToString ();
					break;
				}
			}
		}
	}
}
