using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControls : MonoBehaviour 
{

	public Toggle gyro;
	public Toggle touch;

	GameController gameController;

	private void Start()
    {
		gameController = FindObjectOfType<GameController>();
    }

	public void changeCarControlsTo(bool newValue)
    {
		gameController.isTapOnUI = true;

		Input.gyro.enabled = gyro.isOn;

		Invoke("Reset", 0.2f);
    }

	private void Reset()
	{
		gameController.isTapOnUI = false;
	}

}
