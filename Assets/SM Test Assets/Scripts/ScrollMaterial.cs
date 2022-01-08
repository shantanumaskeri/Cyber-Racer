using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMaterial : MonoBehaviour 
{
	
	public float tunnelTextureTwist = -0.03f;
	public float tunnelTextureSpeed = 0.1f;
    
	public Material mat;

	GameManager gameManager;

	private void Start()
    {
		gameManager = FindObjectOfType<GameManager>();
    }

	private void FixedUpdate()
	{
		if (!gameManager.gameOver)
        {
			tunnelTextureSpeed += 0.0002f * Time.deltaTime;

			float verticalOffset = Time.time * tunnelTextureSpeed;
			float horizontalOffset = Time.time * tunnelTextureTwist;

			mat.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
		}
	}

}
