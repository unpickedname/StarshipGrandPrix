using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRacer : BaseRacer{


	string Horizontal = "Horizontal";
	string Vertical = "Vertical";
	string RollLeft = "RollLeft";
	string RollRight = "RollRight";
	string Boost = "Boost";
	string Brake = "Brake";
	string SelfDestruct = "SelfDestruct";
	public int VerticalInversion = 1;

	protected override void Initialize()
	{
		base.Initialize ();
		isPlayer = true;


	}

	//updates our ships rotation for pitch, yaw, and roll
	protected override void  UpdateRotation()
	{
		float yawAxis = Input.GetAxis (Horizontal);
		float pitchAxis = Input.GetAxis (Vertical) * VerticalInversion;
		Rolling (yawAxis);

		if(useBankingModifier)
			yawAmount += yawSpeed * bankingYawMultiplier * Time.deltaTime * yawAxis;
		else
			yawAmount += yawSpeed  * Time.deltaTime * yawAxis;

		pitchAmount += pitchSpeed * Time.deltaTime * pitchAxis;
		if (pitchAmount > 45)
			pitchAmount = 45;
		if (pitchAmount < -45)
			pitchAmount = -45;
		transform.rotation = Quaternion.Euler (new Vector3 (pitchAmount, yawAmount, transform.rotation.z)) * Quaternion.Euler(HandleRollRotation());

		if (Input.GetButtonDown (SelfDestruct)) {
			currentHealth -= startingHealth;
			StartCoroutine (Death ());
		}
	}


	//sets whether we should roll, and whether we get the banking modifier
	protected override void Rolling(float yawDirection)
	{
		if (Input.GetButton (RollLeft) && !isBankingRight) {
			shouldBankLeft = true;
			shouldBankRight = false;
		} 
		else if (Input.GetButton(RollRight) && !isBankingLeft){
			shouldBankRight = true;
			shouldBankLeft = false;
		}
	else {
		shouldBankLeft = false;
		shouldBankRight = false;
	    }

		if (shouldBankLeft && yawDirection < 0)
			useBankingModifier = true;
		else if (shouldBankRight && yawDirection > 0)
			useBankingModifier = true;
		else
			useBankingModifier = false;
	}

	//sets whether we should brake
	protected override void  Braking()
	{
		if (Input.GetButton (Brake) && !shouldBoost)
			shouldBrake = true;
		else
			shouldBrake = false;
	}

	//sets whether we should boost
	protected override void Boosting()
	{
		if (Input.GetButton (Boost) && !shouldBrake && remaingBoostCooldown <= 0.0f){
			shouldBoost = true;
			remaingBoostCooldown = boostCooldown;
			remainingBoostTime = boostDuration;
		}
	}

	public void AddPlayerNumberPrefix(string prefix)
	{
		 Horizontal = prefix + "Horizontal";
		 Vertical = prefix + "Vertical";
		RollLeft = prefix + "RollLeft";
		 RollRight = prefix + "RollRight";
		 Boost = prefix + "Boost";
		 Brake = prefix + "Brake";
		SelfDestruct = prefix + "SelfDestruct";
	}
		
}
