using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarRotation : MonoBehaviour 
{

    // Update is called once per frame
    private void Update() 
	{
		transform.Rotate(0.0f, 60.0f * Time.deltaTime, 0.0f);
	}

}
