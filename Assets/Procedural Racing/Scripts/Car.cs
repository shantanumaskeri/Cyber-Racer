using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour {
	
	//variables visible in the inspector
	public Rigidbody rb;

	public Transform[] wheelMeshes;
	public WheelCollider[] wheelColliders;
	
	public int rotateSpeed;
	public int rotationAngle;
	public int wheelRotateSpeed;
	
	public Transform[] grassEffects;
	public Transform[] skidMarkPivots;
	public float grassEffectOffset;
	
	public Transform back;
	public float constantBackForce;
	
	public GameObject skidMark;
	public float skidMarkSize;
	public float skidMarkDelay;
	public float minRotationDifference;
	
	public GameObject ragdoll;

	//not in the inspector
	int targetRotation;
	WorldGenerator generator;

	float lastRotation;
	bool skidMarkRoutine;

	CarControls carControls;
	GameController gameController;

	void Start(){
		//find the world generator and start the skid mark coroutine
		generator = GameObject.FindObjectOfType<WorldGenerator>();
		StartCoroutine(SkidMark());

		carControls = FindObjectOfType<CarControls>();
		gameController = FindObjectOfType<GameController>();
	}
	
	void FixedUpdate(){
		//update the skidmark and the grass effects
		UpdateEffects();
	}
	
	void LateUpdate(){
		//for all wheels
		for(int i = 0; i < wheelMeshes.Length; i++){	
			//set the wheel mesh to the position of the wheel collider
			Quaternion quat;
			Vector3 pos;
			
			wheelColliders[i].GetWorldPose(out pos, out quat);
			
			wheelMeshes[i].position = pos;
			
			//rotate the wheel so it looks like we're driving
			wheelMeshes[i].Rotate(Vector3.right * Time.deltaTime * wheelRotateSpeed);
		}
		
		if (carControls.touch.isOn)
        {
			//if the player wants to turn, rotate the car
			if (Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0)
			{
				if (!gameController.isTapOnUI)
                {
					//Debug.Log("LateUpdate: " + Input.mousePosition.y);
					if (Input.mousePosition.y > 125.0f)
                    {
						UpdateTargetRotation();
					}
				}
			}
			else if (targetRotation != 0)
			{
				//else, rotate back to the center
				targetRotation = 0;
			}
		}
		else if (carControls.gyro.isOn)
		{
			//if the player wants to turn, rotate the car
			if (Mathf.Abs(Input.gyro.gravity.x) > 0.2f)
			{
				UpdateTargetRotation();
			}
			else if (targetRotation != 0)
			{
				//else, rotate back to the center
				targetRotation = 0;
			}
		}

		//apply the rotation by rotating towards the target angle on the y axis
		Vector3 rotation = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotateSpeed * Time.deltaTime);
	}

	void UpdateTargetRotation()
	{
		if (carControls.touch.isOn)
        {
			if (!gameController.isTapOnUI)
            {
				//if we're using the mouse to rotate
				if (Input.GetAxis("Horizontal") == 0)
				{
					//Debug.Log("UpdateTargetRotation: " + Input.mousePosition.y);
					if (Input.mousePosition.y > 125.0f)
                    {
						//get the mouse position (left/right side of the screen)
						if (Input.mousePosition.x > Screen.width * 0.5f)
						{
							//rotate right
							targetRotation = rotationAngle;
						}
						else
						{
							//rotate left
							targetRotation = -rotationAngle;
						}
					}
				}

				if (Input.GetAxis("Horizontal") != 0)
				{
					//condition.text = "inside 2nd if condition";
					//if we're pressing arrow keys or a/d, rotate the car based on the horizontal input value
					targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
				}
			}
		}
		else if (carControls.gyro.isOn)
		{
			if (Mathf.Abs(Input.gyro.gravity.x) > 0.2f)
			{
				//if we're pressing arrow keys or a/d, rotate the car based on the horizontal input value
				targetRotation = (int)(rotationAngle * Input.gyro.gravity.x) * 2;
			}
		}
	}
	
	void UpdateEffects(){
		//if both wheels are off the ground, add force will be true
		bool addForce = true;
		//check if we rotated the car since last frame
		bool rotated = Mathf.Abs(lastRotation - transform.localEulerAngles.y) > minRotationDifference;
		
		//for both grass effects (rear wheels)
		for(int i = 0; i < 2; i++){
			//get the rear wheels (one of them in each iteration)
			Transform wheelMesh = wheelMeshes[i + 2];
			
			//check if this wheel is grounded currently
			if(Physics.Raycast(wheelMesh.position, Vector3.down, grassEffectOffset * 1.5f)){
				//if so, show the grass effect
				if(!grassEffects[i].gameObject.activeSelf)
					grassEffects[i].gameObject.SetActive(true);
				
				//update the grass effect height and the skidmark height to match this wheel
				float effectHeight = wheelMesh.position.y - grassEffectOffset;
				Vector3 targetPosition = new Vector3(grassEffects[i].position.x, effectHeight, wheelMesh.position.z);
				grassEffects[i].position = targetPosition;
				skidMarkPivots[i].position = targetPosition;
				
				//this wheel is grounded so we don't need any extra force at the back of the car
				addForce = false;
			}
			else if(grassEffects[i].gameObject.activeSelf){
				//if we're not grounded, don't show the grass effect
				grassEffects[i].gameObject.SetActive(false);
			}
		}
		
		//add force at the back of the car for stabilization
		if(addForce){
			rb.AddForceAtPosition(back.position, Vector3.down * constantBackForce);
			//don't show the skidmarks
			skidMarkRoutine = false;
		}
		else{
			if(targetRotation != 0){
				//if the car has rotated show the skid mark
				if(rotated && !skidMarkRoutine){
					skidMarkRoutine = true;
				}
				else if(!rotated && skidMarkRoutine){
					skidMarkRoutine = false;
				}
			}
			else{
				//don't show the skidmark if we're rotating back to the center
				skidMarkRoutine = false;
			}
		}
		
		//update the last rotation (which is now the current rotation since everything has been updated)
		lastRotation = transform.localEulerAngles.y;
	}
	
	public void FallApart(){
		//destroy the car
		Instantiate(ragdoll, transform.position, transform.rotation);
		gameObject.SetActive(false);
	}
	
	IEnumerator SkidMark(){
		//loops continuesly
		while(true){
			//wait for the delay in between individual skid marks
			yield return new WaitForSeconds(skidMarkDelay);
			
			//show skidmarks if we need skidmarks now
			if(skidMarkRoutine){
				//for both rear wheels, instantiate a skid mark and parent it to the environment so it moves realistically
				for(int i = 0; i < skidMarkPivots.Length; i++){
					GameObject newskidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
					newskidMark.transform.parent = generator.GetWorldPiece();
					newskidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
				}
			}
		}
	}
}
