using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour 
{
	
	public Animator UIAnimator;
	public string NextScene;

	GameController gameController;
	InstructionsPage instructionsPage;
	Scene currentScene;
	string sceneName;

	private void Start()
    {
		gameController = FindObjectOfType<GameController>();

		currentScene = SceneManager.GetActiveScene();
		sceneName = currentScene.name;

		if (sceneName == "Instructions")
        {
			instructionsPage = FindObjectOfType<InstructionsPage>();
		}
    }

	private void Update()
	{
		if (Input.touchCount == 1)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				if (!gameController.isTapOnUI)
                {
					if (sceneName == "Intro")
                    {
						StartGame();
					}
					else if (sceneName == "Instructions")
                    {
						if (instructionsPage.page1.activeSelf)
						{
							instructionsPage.page1.SetActive(false);
							instructionsPage.page2.SetActive(true);
						}
						else if (instructionsPage.page2.activeSelf)
						{
							StartGame();
						}
					}
				}
			}
		}
	}

	public void StartGame()
	{
		//fade to black and load the next scene
		UIAnimator.SetTrigger("Start");
		StartCoroutine(LoadScene(NextScene));
	}
	
	//wait less than a second and load the given scene
	IEnumerator LoadScene(string scene)
	{
		yield return new WaitForSeconds(0.6f);
		
		SceneManager.LoadScene(scene);
	}

}