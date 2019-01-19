using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
	public GameObject ship;
	public float zOffset;
	public float yOffset;
	public float xOffset;
	public float brakingOffset;
	public float boostingOffset;
	public float speedPowerUpOffset;
	public float maxHeight;
	public float minHeight;
	public bool shouldEnforceHeightRestrictions;
	public float zMovementSpeed;
	public float zReturnToDefaultSpeed;
	public float xMovementSpeed;
	public float xCameraSensitivity;
	public float xReturnToDefaultSpeed;
	public float yMovementSpeed;
	public float yCameraSensitivity;
	public float yReturnToDefaultSpeed;
	public float cameraShakeMagnitude;
	public float cameraShakeFrequency;
	public bool shakeFlag;
	public string playerPrefix;
	float currentZOffset;
	float currentYOffset;
	float currentXOffset;
	float cameraShakeFrequencyTimer;
	Transform startingTransform;

	// Use this for initialization
	void Start () {
		currentZOffset = zOffset;
		currentYOffset = 0;
		currentXOffset = 0;

	  
		startingTransform = this.transform;

		transform.position = ship.transform.position -ship.transform.forward * currentZOffset;
		transform.position = transform.position + transform.up * yOffset;
		transform.rotation = Quaternion.Euler(ship.transform.eulerAngles.x,ship.transform.eulerAngles.y, 0);
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	void LateUpdate()
	{ 
		string racerState = ship.GetComponent<BaseRacer> ().GetCurrentState ();
		if (racerState != "PreRace" && racerState != "PostRace") {
			transform.rotation = Quaternion.Euler (ship.transform.eulerAngles.x, ship.transform.eulerAngles.y, 0);

			MoveCameraToOffset ();
			if (shouldEnforceHeightRestrictions)
				EnforceHeightRestrictions ();

			if (ship.GetComponent<PlayerRacer>().shouldShakeCamera) {
				Shake ();
			} else
				cameraShakeFrequencyTimer = 0.0f;
		}

	}


	void FixedUpdate()
	{


	}

	void MoveCameraToOffset()
	{ 
		FindZOffset ();
		transform.position = ship.transform.position -ship.transform.forward * currentZOffset;

		FindYoffset ();
		transform.position = transform.position + transform.up * yOffset;

		FindXoffset ();

		transform.position = transform.position - transform.right * currentXOffset;




	}

	void FindXoffset()
	{
		   //Instead of greater than 0, perhaps greater than a certain amount for hard turns?
		if (Input.GetAxis (playerPrefix + "Horizontal") < -xCameraSensitivity) {
			currentXOffset += xMovementSpeed * Time.deltaTime;

			if (currentXOffset > xOffset)
				currentXOffset = xOffset;
		} else if (Input.GetAxis (playerPrefix + "Horizontal") > xCameraSensitivity) {
			currentXOffset -= xMovementSpeed * Time.deltaTime;

			if (currentXOffset < -xOffset)
				currentXOffset = -xOffset;
			
		} else {//handles returning to default after turning
			if (currentXOffset < 0) {
				currentXOffset += xReturnToDefaultSpeed * Time.deltaTime;

				if (currentXOffset > 0)
					currentXOffset = 0;
			} else if (currentXOffset > 0) {
				currentXOffset -= xReturnToDefaultSpeed * Time.deltaTime;

				if (currentXOffset < 0)
					currentXOffset = 0;
			}
			
		}
			
	}



	void FindYoffset()
	{
		//Instead of greater than 0, perhaps greater than a certain amount for hard turns?
		if (Input.GetAxis (playerPrefix + "Vertical") < -yCameraSensitivity) {
			currentYOffset += yMovementSpeed * Time.deltaTime;

			if (currentYOffset > yOffset)
				currentYOffset = yOffset;
		} else if (Input.GetAxis (playerPrefix  + "Vertical") > yCameraSensitivity) {
			currentYOffset -= yMovementSpeed * Time.deltaTime;

			if (currentYOffset < -yOffset)
				currentYOffset = -yOffset;

		} else {//handles returning to default after turning
			if (currentYOffset < 0) {
				currentYOffset += yReturnToDefaultSpeed * Time.deltaTime;

				if (currentYOffset > 0)
					currentYOffset = 0;
			} else if (currentYOffset > 0) {
				currentYOffset -= yReturnToDefaultSpeed * Time.deltaTime;

				if (currentYOffset < 0)
					currentYOffset = 0;
			}

		}

	}

	void FindZOffset()
	{
		if (ship.GetComponent<PlayerRacer> ().IsBraking () && !ship.GetComponent<PlayerRacer> ().SpeedPowerUpActive ()) {
			currentZOffset -= zMovementSpeed * Time.deltaTime;

			if (currentZOffset < brakingOffset)
				currentZOffset = brakingOffset;
		} else if (ship.GetComponent<PlayerRacer> ().IsBoosting () && !ship.GetComponent<PlayerRacer> ().SpeedPowerUpActive ()) {
			currentZOffset += zMovementSpeed * Time.deltaTime;
			if (currentZOffset > boostingOffset)
				currentZOffset = boostingOffset;
		} else if (ship.GetComponent<PlayerRacer> ().SpeedPowerUpActive ()) {
			currentZOffset += zMovementSpeed * Time.deltaTime;
			if (currentZOffset > speedPowerUpOffset)
				currentZOffset = speedPowerUpOffset;

		}
		else {//handles returning to default zOffset from boosting/braking
			if (currentZOffset < zOffset) {
				currentZOffset += zReturnToDefaultSpeed * Time.deltaTime;

				if (currentZOffset > zOffset)
					currentZOffset = zOffset;
			} else if (currentZOffset > zOffset) {
				currentZOffset -= zReturnToDefaultSpeed * Time.deltaTime;

				if (currentZOffset < zOffset)
					currentZOffset = zOffset;
			}
		}


	}

	void EnforceHeightRestrictions()
	{
		if (transform.position.y > maxHeight) {
			Vector3 currentPosition = transform.position;
			currentPosition.y = maxHeight;
			transform.position = currentPosition;
		}
		if (transform.position.y < minHeight) {
			Vector3 currentPosition = transform.position;
			currentPosition.y = minHeight;
			transform.position = currentPosition;
		}
	}

	void Shake()
	{
		float x = Random.Range (-1, 1) * cameraShakeMagnitude;
		float y = Random.Range (-1, 1) * cameraShakeMagnitude;
		cameraShakeFrequencyTimer += Time.deltaTime;

		if (cameraShakeFrequencyTimer >= cameraShakeFrequency) {
			transform.position = new Vector3 (transform.position.x + x, transform.position.y + y, transform.position.z);
			cameraShakeFrequencyTimer = 0.0f;
		}
	}
}
