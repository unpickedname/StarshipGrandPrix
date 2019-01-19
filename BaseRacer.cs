using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRacer : MonoBehaviour {


	public GameObject[] course;
	protected GameObject target;
	public float pitchSpeed;
	public float yawSpeed;
	public float maxSpeed;
	public float maxBoostingSpeed;
	public float forwardAcceleration;
	public float brakingAcceleration;
	public float boostingAcceleration;
	public float bankingSpeed;
	public float shouldBrakeThreshold;
	public float shouldRollThreshold;
	public float boostDistanceThreshold;
	public float boostDuration;
	public float boostCooldown;
	public float minSpeed;
	public float bankingYawMultiplier;
	public float engineSoundSpeed = 250;
	public int startingHealth;
	public float InvulnerabilityTime;
	public float RespawnTime;
	public float powerUpBoostAcceleration;
	public float powerUpBoostMaxSpeed;
	public float powerUpBoostTime;
	public bool shouldShakeCamera;
	public GameObject Explosion;
	protected bool shouldBrake;
	protected bool shouldBoost;
	protected bool shouldBankLeft;
	protected bool shouldBankRight;
	protected bool isBankingLeft;
	protected bool isBankingRight;
	protected bool useBankingModifier;
	protected float bankingAmount;
	protected float yawAmount;
	protected float pitchAmount;
	protected float remainingBoostTime;
	protected float remaingBoostCooldown;
	protected float currentVelocity;
	protected int currentHealth;
	protected Vector3 respawnPoint;
	protected Quaternion respawnRotation;
	protected int respawnIndex;
	protected bool canBeDamaged;
	protected bool isPlayer;
	protected enum RacerState {PreRace , Racing, Dead, PostRace};
	RacerState rs;
	int courseIndex;
	Rigidbody rb;
	GoalHandler gl;
	Renderer myRenderer;
	Material originalMaterial;
	public Material collisionMaterial;
	AudioSource engineSound;
	bool isSpeedPowerUpActive;


	// Use this for initialization
	void Start () {
		Initialize ();

	}

	// Update is called once per frame
	void Update () {


		if (rs == RacerState.Racing) {
			
				UpdateRotation ();
				Braking ();
				Boosting ();
				BoostCooldownHandling ();


			UpdateSound ();
		}



	}

	void FixedUpdate()
	{
		if (rs == RacerState.Racing) {

				CalculateVelocity ();
		}
	}

	protected virtual void UpdateSound()
	{

		if (engineSound) {
			float currentEnginePitch = GetCurrentVelocity () / engineSoundSpeed;
			engineSound.pitch = Mathf.Clamp (currentEnginePitch, 0.1f, 1.0f);
		}
	}

	protected virtual void Initialize()
	{
		rb = GetComponent<Rigidbody> ();
		courseIndex = 0;
		target = course [courseIndex];
		shouldBrake = false;
		shouldBoost = false;
		useBankingModifier = false;
		remainingBoostTime = 0.0f;
		remaingBoostCooldown = 0.0f;
		isPlayer = false;
		myRenderer = this.GetComponent<Renderer> ();
		originalMaterial = myRenderer.material;
		gl = this.GetComponent<GoalHandler> ();
		isSpeedPowerUpActive = false;

		currentHealth = startingHealth;
		engineSound = this.GetComponent<AudioSource> ();
		canBeDamaged = true;
		respawnIndex = 0;
		respawnPoint = this.transform.position;
		rs = RacerState.PreRace;

		if (gl) {
			foreach (GameObject g in course) {
				
				if (g.tag == "goal") {
					gl.SetInitialTarget (g);
					break;

				}
			}
		}
	}

	protected virtual void HandleTriggerEnter(Collider col)
	{

		if (rs == RacerState.Racing) {
			if (col.gameObject == target.gameObject) {

				++courseIndex;

				if (courseIndex >= course.Length)
					courseIndex = 0;

				target = course [courseIndex];

				if (gl && col.gameObject.tag == "goal") {
					gl.SetNextTarget (target, isPlayer);
					respawnPoint = col.transform.position;
					respawnRotation = this.transform.rotation;
					respawnIndex = courseIndex;

				}

			}

			if (col.gameObject.tag == "obstacle" && canBeDamaged && rs == RacerState.Racing) {
				--currentHealth;
				StartCoroutine (CollisionFlash ());
				StartCoroutine (InvulnerabilityPeriod ());
				currentVelocity = minSpeed;

				if (currentHealth <= 0 && rs == RacerState.Racing) {
				
					StartCoroutine (Death ());
				}
			}
		}
	}



	//updates our ships rotation for pitch, yaw, and roll
	protected virtual void  UpdateRotation()
	{

	}


	//sets whether we should roll, and whether we get the banking modifier
	protected virtual void Rolling(float yawDirection)
	{
	}

	//sets whether we should brake
	protected virtual void  Braking()
	{
	}

	//sets whether we should boost
	protected virtual void Boosting()
	{
	}

	public  bool IsBoosting()
	{
		return shouldBoost;
	}

	public  bool IsBraking()
	{
		return shouldBrake;
	}

	public float GetCurrentVelocity()
	{
		return currentVelocity;
	}

	public float GetBoostCooldown()
	{
		return remaingBoostCooldown;
	}

	public void StartRace()
	{
		rs = RacerState.Racing;
	}

	public void EndRace()
	{
		rs = RacerState.PostRace;
	}

	public bool IsPlayer()
	{
		return isPlayer;
	}
		

	public int GetHealth()
	{
		return currentHealth;
	}

	public string GetCurrentState()
	{
		return rs.ToString ();
	}

	public bool SpeedPowerUpActive()
	{
		return isSpeedPowerUpActive;
	}

	public void ActivateBoostPowerUp()
	{
		StartCoroutine (BoostPowerUp());

	}

	IEnumerator BoostPowerUp()
	{
		isSpeedPowerUpActive = true;

		yield return new WaitForSeconds (powerUpBoostTime);

		isSpeedPowerUpActive = false;
	}

    //returns a vector3 that tells us our z-rotation
	protected Vector3 HandleRollRotation()
	{
		Vector3 bankRotation = new Vector3 (0, 0, 0);

		if (shouldBankLeft && !isBankingRight) {
			isBankingLeft = true;
			bankingAmount += bankingSpeed * Time.deltaTime;
			if (bankingAmount > 90)
				bankingAmount = 90;
			bankRotation = new Vector3 (0, 0, bankingAmount);

		} else if (shouldBankRight && !isBankingLeft) {
			isBankingRight = true;
			bankingAmount += -bankingSpeed * Time.deltaTime;
			if (bankingAmount < -90)
				bankingAmount = -90;
			bankRotation = new Vector3 (0, 0, bankingAmount);
		} 

		//rotate back to original orientation
		if (!shouldBankLeft && isBankingLeft) {
			bankingAmount += -bankingSpeed * Time.deltaTime;

			if (bankingAmount < 0) {
				bankingAmount = 0;
				isBankingLeft = false;
			}
			bankRotation = new Vector3 (0, 0, bankingAmount);
		}

		if (!shouldBankRight && isBankingRight) {
			bankingAmount += bankingSpeed * Time.deltaTime;

			if (bankingAmount > 0) {
				bankingAmount = 0;
				isBankingRight = false;
			}
			bankRotation = new Vector3 (0, 0, bankingAmount);
		}

		return bankRotation;
	}

	//handles how long boost last, and when we can use it again
	protected void BoostCooldownHandling()
	{
		if (remainingBoostTime > 0.0f) {
			remainingBoostTime -= Time.deltaTime;
		} else
			shouldBoost = false;

		if (remaingBoostCooldown > 0.0f)
			remaingBoostCooldown -= Time.deltaTime;
	}
		
	//based on whether we should boost or brake, determines how fast ship is moving
	protected void CalculateVelocity()
	{
		if (shouldBrake)
			currentVelocity -= brakingAcceleration;
		else if (shouldBoost)
			currentVelocity += boostingAcceleration;
		else
			currentVelocity += forwardAcceleration;

		if (currentVelocity > maxSpeed && !shouldBoost  && !isSpeedPowerUpActive) {

			currentVelocity -= boostingAcceleration;

			if(currentVelocity < maxSpeed)
			   currentVelocity = maxSpeed;
		}
	

		if (currentVelocity > maxBoostingSpeed && shouldBoost && !isSpeedPowerUpActive)
			currentVelocity = maxBoostingSpeed;
		if (currentVelocity < minSpeed)
			currentVelocity = minSpeed;

		if (isSpeedPowerUpActive) {

			currentVelocity += powerUpBoostAcceleration;

			if (currentVelocity > powerUpBoostMaxSpeed)
				currentVelocity = powerUpBoostMaxSpeed;
		}

		rb.velocity = currentVelocity * transform.forward;
	}

	void OnTriggerEnter(Collider col)
	{
		HandleTriggerEnter (col);
	}

	IEnumerator CollisionFlash()
	{
		shouldShakeCamera = true;
		myRenderer.material = collisionMaterial;
		yield return  new WaitForSeconds (0.1f);
		myRenderer.material = originalMaterial;
		yield return  new WaitForSeconds (0.1f);
		myRenderer.material = collisionMaterial;
		yield return  new WaitForSeconds (0.1f);
		myRenderer.material = originalMaterial;
		yield return  new WaitForSeconds (0.1f);
		myRenderer.material = collisionMaterial;
		yield return  new WaitForSeconds (0.1f);
		myRenderer.material = originalMaterial;

		shouldShakeCamera = false;

	}

	IEnumerator InvulnerabilityPeriod()
	{
		canBeDamaged = false;
		myRenderer.material = collisionMaterial;
		yield return  new WaitForSeconds (InvulnerabilityTime);
		myRenderer.material = originalMaterial;


		canBeDamaged = true;
	}


	public IEnumerator Death()
	{
		rs = RacerState.Dead;
		Instantiate (Explosion, transform.position, transform.rotation);
		myRenderer.enabled = false;
		rb.velocity = Vector3.zero;
		canBeDamaged = false;
		engineSound.Stop ();
		yield return  new WaitForSeconds (RespawnTime);
		transform.position = respawnPoint;
		courseIndex = respawnIndex;
		target = course [courseIndex];
		currentHealth = startingHealth;
		canBeDamaged = true;
		myRenderer.enabled = true;
		engineSound.Play ();
		rs = RacerState.Racing;


	}



}
