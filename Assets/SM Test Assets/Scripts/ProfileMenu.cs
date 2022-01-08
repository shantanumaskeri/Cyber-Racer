using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileMenu : MonoBehaviour 
{

    public Animator UIAnimator;

	private void FixedUpdate()
    {
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GameObject go = hit.transform.gameObject;
                    if (go.name == "Car")
                    {
                        UIAnimator.SetTrigger("Start");
                        StartCoroutine(LoadScene("Game"));
                    }
                }
            }
        }
    }

    //wait less than a second and load the given scene
    IEnumerator LoadScene(string scene)
    {
        yield return new WaitForSeconds(0.6f);

        SceneManager.LoadScene(scene);
    }

}
