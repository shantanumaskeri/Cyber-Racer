using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour 
{

	[HideInInspector]
	public bool isTapOnUI = false;

	// Update is called once per frame
	private void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
        {
			Application.Quit();
        }
	}

}
