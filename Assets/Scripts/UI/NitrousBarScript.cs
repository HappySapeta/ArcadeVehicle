using Vehicle;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace UI
{
	public class NitrousBarScript : MonoBehaviour
	{
		[FormerlySerializedAs("car")] [SerializeField]
		private VehicleMovement vehicle;

		private Image nitrousBar;
		
		void Awake ()
		{
			nitrousBar = GetComponent<Image> ();
		}
	
		void Update ()
		{
			nitrousBar.fillAmount = vehicle.boostAmt / 100;
		}
	}
}
