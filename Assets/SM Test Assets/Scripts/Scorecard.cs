using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorecard : MonoBehaviour 
{
	
	public string objectName;
	public string colorValue;
	
	//not visible in the inspector
	GameManager manager;
	bool addedScore;
	GameObject score;
	AudioSource source;

	private void Start()
	{
		//find the game manager
		manager = GameObject.FindObjectOfType<GameManager>();
		score = GameObject.Find("/Score");
		source = score.GetComponent<AudioSource>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		//check if the player drove through this gate and we didn't yet add any points
		if (!other.gameObject.transform.root.CompareTag("Player") || addedScore || manager.gameOver)
        {
			return;
		}
			
		//increase the score by one and play some audio
		addedScore = true;
		source.Play();

		switch (objectName)
        {
			case "gate":
				manager.UpdateScore(1, colorValue);
				break;

			case "coin":
				manager.UpdateScore(2, colorValue);
				Destroy(gameObject);
				break;

			case "stone":
				manager.UpdateScore(2, colorValue);
				Destroy(gameObject);
				break;
		}
	}
}
