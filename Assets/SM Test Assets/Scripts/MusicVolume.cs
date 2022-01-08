using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolume : MonoBehaviour 
{

	GameController gameController;

	private void Start()
	{
		gameController = FindObjectOfType<GameController>();

		CheckVolume();
	}

	private void CheckVolume()
    {
		//get the wanted audio level
		int audio = PlayerPrefs.GetInt("Audio");
		//get the opposite (if the player prefs are resetted, we do want to have sound by default)
		audio = (audio == 0) ? 1 : 0;
		//set the game volume and show the red line
		AudioListener.volume = audio;

		if (AudioListener.volume == 0)
		{
			gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}
		else
		{
			gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
	}

	public void Mute()
	{
		gameController.isTapOnUI = true;

		//set the audio volume to the opposite of our current volume
		if (AudioListener.volume == 0)
		{
			AudioListener.volume = 1;
			gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
		else
		{
			AudioListener.volume = 0;
			gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		}

		//update the player prefs value for the music
		PlayerPrefs.SetInt("Audio", ((int)AudioListener.volume == 0) ? 1 : 0);

		Invoke("Reset", 0.2f);
	}

	private void Reset()
    {
		gameController.isTapOnUI = false;
    }

}
