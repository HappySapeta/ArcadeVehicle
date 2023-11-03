using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal enum playerType
{
	human_pc,
	human_mobile,
	bot
}

public class InputManager : MonoBehaviour
{
	[SerializeField]private playerType _playerType;


	public float Horizontal { get; set; }

	public float forward{ get; set; }

	public float backward{ get; set; }

	void Start ()
	{
		
	}


	void Update ()
	{
		if (_playerType == playerType.human_pc) {
			

			Horizontal = Input.GetAxis ("Horizontal");
			forward = (Input.GetAxis ("Vertical") > 0) ? Input.GetAxis ("Vertical") : 0;
			backward = (Input.GetAxis ("Vertical") < 0) ? Input.GetAxis ("Vertical") : 0;
		} else if (_playerType == playerType.human_mobile) {
			forward = (Input.acceleration.y > 0) ? Input.acceleration.y : 0;
			backward = (Input.acceleration.y < 0) ? Input.acceleration.y : 0;
		} else {
			// do nothing.
			// AI script provides input.
		}
	}

	public int ReturnPlayerType ()
	{
		if (_playerType == playerType.bot)
			return 2;
		else if (_playerType == playerType.human_mobile)
			return 1;
		else
			return 0;
	}
}
