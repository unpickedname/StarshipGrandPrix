using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIRacer : BaseRacer{

	public float courseWeight;
	public float obstacleAvoidanceWeight;
	public float avoidanceRaycastWidth;
	bool obstacleInBoostRange;

	//updates our ships rotation for pitch, yaw, and roll
	protected override void  UpdateRotation()
	{
		Vector3 directionToTarget = target.transform.position - transform.position;

		directionToTarget =  courseWeight *directionToTarget.normalized;

		RaycastHit hit;

		Vector3 leftR = transform.position;
		Vector3 rightR = transform.position;

		leftR.x -= avoidanceRaycastWidth;
		rightR.x += avoidanceRaycastWidth;

		if (Physics.Raycast (transform.position, transform.forward, out hit, 75)) {

			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.DrawLine (transform.position, hit.point, Color.blue);
				directionToTarget += (obstacleAvoidanceWeight * hit.normal);
				obstacleInBoostRange = true;
			
			} 
		} else if (Physics.Raycast (leftR, transform.forward, out hit, 75)) {
			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.DrawLine (leftR, hit.point, Color.red);
				directionToTarget += (obstacleAvoidanceWeight * hit.normal);
				obstacleInBoostRange = true;
			}
		} else if (Physics.Raycast (rightR, transform.forward, out hit, 75)) {
			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.DrawLine (rightR, hit.point, Color.red);
				directionToTarget += (obstacleAvoidanceWeight * hit.normal);
				obstacleInBoostRange = true;
			}
		} else {
			obstacleInBoostRange = false;
		}

		Quaternion lookToTargetRotation = Quaternion.FromToRotation (transform.forward, directionToTarget);
		Vector3 eulerLooktoTargetRotation = lookToTargetRotation.eulerAngles;
		float pitchDirection = eulerLooktoTargetRotation.x;
		float yawDirection = eulerLooktoTargetRotation.y;

		//since we are using these to add to the pitch amount, we want to make sure the numbers are between +/- 180 so we can know if the desired direction is above or below us
		if (pitchDirection > 180)
			pitchDirection -= 360;
		if (pitchDirection < -180)
			pitchDirection += 360;
		//since we are using these to add to the yaw amount, we want to make sure the numbers are between +/- 180 so we can know if the desired direction is left or right
		if (yawDirection > 180)
			yawDirection -= 360;

		Rolling (yawDirection);

		if(useBankingModifier)
			yawAmount += Time.deltaTime * yawSpeed* bankingYawMultiplier * Mathf.Clamp (yawDirection - transform.rotation.y, -1, 1);
		else
			yawAmount += Time.deltaTime * yawSpeed * Mathf.Clamp (yawDirection - transform.rotation.y, -1, 1);

		//when we are turned around on our y axis, we have to flip the pitch amounts
		float xTurnedAround = yawAmount%360;

		if (xTurnedAround > 180)
			xTurnedAround -= 360;

		if(xTurnedAround < -90 || xTurnedAround > 90 )
			pitchAmount +=  Time.deltaTime * pitchSpeed * -Mathf.Clamp (pitchDirection - transform.rotation.x, -1, 1);
		else
			pitchAmount +=  Time.deltaTime * pitchSpeed * Mathf.Clamp (pitchDirection - transform.rotation.x, -1, 1);

		Mathf.Clamp (pitchAmount, -45, 45);
		transform.rotation = Quaternion.Euler (new Vector3 (pitchAmount, yawAmount, transform.rotation.z)) * Quaternion.Euler(HandleRollRotation());
	}


	//sets whether we should roll, and whether we get the banking modifier
	protected override void Rolling(float yawDirection)
	{
		if (Vector3.Angle (transform.forward, target.transform.position - transform.position) > shouldRollThreshold) {

			if (yawDirection < 0.0f) {
				shouldBankLeft = true;
				shouldBankRight = false;
				useBankingModifier = true;
			} else {
				shouldBankRight = true;
				shouldBankLeft = false;
				useBankingModifier = true;
			}
		} else {
			shouldBankLeft = false;
			shouldBankRight = false;
			useBankingModifier = false;
		}
	}

	//sets whether we should brake
	protected override void  Braking()
	{
		if (Vector3.Angle (transform.forward,target.transform.position - transform.position ) > shouldBrakeThreshold)
			shouldBrake = true;
		else
			shouldBrake = false;
	}

	//sets whether we should boost
	protected override void Boosting()
	{
		if (!shouldBrake && Vector3.Distance (target.transform.position, transform.position) > boostDistanceThreshold && remaingBoostCooldown <= 0.0f && !obstacleInBoostRange) {
			shouldBoost = true;
			remaingBoostCooldown = boostCooldown;
			remainingBoostTime = boostDuration;
		}
	}

	Vector3 DetectObstacleToAvoid()
	{
		RaycastHit hit;
		Vector3 leftRaycast = transform.position;
		leftRaycast.x -= 5;
		Vector3 rightRaycast = transform.position;
		rightRaycast.x += 5;


		if (Physics.Raycast (transform.position, transform.forward, out hit, 50)) {

			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.Log ("obstacle");
				Debug.DrawLine(transform.position, hit.point, Color.blue);

				return hit.normal;
			} 

			  else {
				Debug.Log ("no obstacle");
				return Vector3.zero;
			}
		} 
		else if (Physics.Raycast (leftRaycast, transform.forward, out hit, 50)) {

			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.Log (" left obstacle");
				Debug.DrawLine(leftRaycast, hit.point, Color.blue);

				return hit.normal;
			} 

			else {
				Debug.Log ("no obstacle");
				return Vector3.zero;
			}
		} 

		else if (Physics.Raycast (rightRaycast, transform.forward, out hit, 50)) {

			if (hit.collider.gameObject.tag == "obstacle") {
				Debug.Log (" right obstacle");
				Debug.DrawLine(rightRaycast, hit.point, Color.blue);

				return hit.normal;
			} 

			else {
				Debug.Log ("no obstacle");
				return Vector3.zero;
			}
		} 


		else {
			Debug.Log ("no obstacle");
			return Vector3.zero;
		}
	}

}
