using UnityEngine;  
using System.Collections; 


public class CarController : MonoBehaviour {
	public bool isMobile;
	public Transform centerOfMass;
	public ParticleSystem wheelParticle;
	public bool useSmoke;
	public ParticleSystem smoke;
	public float smokeTargetSpeed = 17f;
	public float cameraDistance = 15f;

	public DrivetrainType drivetrain = DrivetrainType.RWD;
	public WheelJoint2D frontMotorWheel;
	public WheelJoint2D rearMotorWheel;

	// Store car speed
	public float speed;
	// How much distance of the ground means cars is grounded? answer=> less than 2.1f
	public float groundDistance = 2.1f ;
	// Motor power, Brake power and deceleration speed
	public float motorPower = 1400f,
	brakePower = -14f,
	decelerationSpeed = 0.3f;
	// Car max speed
	public float maxSpeed = 14f;
	// Rotate force on the  fly 
	public float RotateForce = 140f;

	JointMotor2D rearMotor;
	JointMotor2D frontMotor;
	// Store is grounded based on the distance of the ground
	bool isGrounded;
	// inrenal usage
	float motorTemp;
	// Can rotate option. be true value when car is on the fly
	bool canRotate = false;

	// Internal usage
	float powerTemp;
	ParticleSystem.EmissionModule em ,emSmoke;
	public Transform particlePosition;


	[HideInInspector]public AudioSource EngineSoundS;

	void Awake()
	{
		Vector3 posCamera;
		posCamera = Camera.main.transform.position;
		Camera.main.transform.position = new Vector3 (posCamera.x, posCamera.y, - cameraDistance);
	}

	void UpdateDriveTrain()
    {
		if (drivetrain == DrivetrainType.RWD)
		{
			rearMotorWheel.useMotor = true;
			rearMotor = rearMotorWheel.motor;
			frontMotorWheel.useMotor = false;
		}
		else if (drivetrain == DrivetrainType.FWD)
		{
			frontMotorWheel.useMotor = true;
			frontMotor = frontMotorWheel.motor;
			rearMotorWheel.useMotor = false;
		}
		else if (drivetrain == DrivetrainType.AWD)
		{
			frontMotorWheel.useMotor = true;
			rearMotorWheel.useMotor = true;
			frontMotor = frontMotorWheel.motor;
			rearMotor = rearMotorWheel.motor;
		}

	}

	void Start () { 
		// Set car rigidbody's COM
		GetComponent<Rigidbody2D>().centerOfMass = centerOfMass.transform.localPosition;
		UpdateDriveTrain();


		// Cast a ray to find isGrounded 
		StartCoroutine (RaycCast ());

		EngineSoundS = GetComponent<AudioSource> ();

		powerTemp = motorPower;

		em = wheelParticle.emission;
		em.enabled = false;

		if (smoke) {
			emSmoke = smoke.emission;
			emSmoke.enabled = false;
		}



	}  

	float currentSpeed;    
	//float maxspeed = 300f;

	void FixedUpdate(){

		// speed limiter based on max speed limit value
		if (speed > maxSpeed)
			motorPower = 0;
		else
			motorPower = powerTemp;
		
		// Moving forward
		if (Input.GetAxis ("Horizontal") > 0 || HoriTemp > 0) {
			// Add force to car back wheel
			if(isGrounded)
				rearMotor.motorSpeed = Mathf.Lerp (rearMotor.motorSpeed, -motorPower, Time.deltaTime * 1.4f);
				frontMotor.motorSpeed = Mathf.Lerp (frontMotor.motorSpeed, -motorPower, Time.deltaTime * 1.4f);

			// Wheel particles
			if (isGrounded) {
				if (speed < 4.3f) {
					wheelParticle.transform.position = particlePosition.position;

					em.enabled = true;

				} else
					em.enabled = false;
			}
			else
				em.enabled = false;
			
		}
		else 
		{// Moving backward
			if (Input.GetAxis ("Horizontal") < 0 || HoriTemp < 0) {

				if (speed < -maxSpeed) {
					if (isGrounded)
						rearMotor.motorSpeed = Mathf.Lerp (rearMotor.motorSpeed, 0, Time.deltaTime * 3f);
						frontMotor.motorSpeed = Mathf.Lerp (frontMotor.motorSpeed, 0, Time.deltaTime * 3f);
				} else {
					if (isGrounded)
						rearMotor.motorSpeed = Mathf.Lerp (rearMotor.motorSpeed, motorPower, Time.deltaTime * 1.4f);
						frontMotor.motorSpeed = Mathf.Lerp (frontMotor.motorSpeed, motorPower, Time.deltaTime * 1.4f);
				}
                                                                                      
			} else {// Releasing car throttle and brake
				if (isGrounded)
					rearMotor.motorSpeed = Mathf.Lerp (rearMotor.motorSpeed, 0, Time.deltaTime * decelerationSpeed);
					frontMotor.motorSpeed = Mathf.Lerp (frontMotor.motorSpeed, 0, Time.deltaTime * decelerationSpeed);	
			}

		}

		// Update WheelJoint2D motor inputs

		if(drivetrain == DrivetrainType.RWD || drivetrain == DrivetrainType.AWD)
        {
			rearMotorWheel.motor = rearMotor; 
        }
		if (drivetrain == DrivetrainType.FWD || drivetrain == DrivetrainType.AWD)
		{
			frontMotorWheel.motor = frontMotor;
		}

		// Cheack fo rotate on the fly

		Rotate ();
	
		#if UNITY_EDITOR
		EngineSoundEditor ();  
		#else
		EngineSoundMobile (); 
		#endif

		if (!isMobile)
			HoriTemp = Input.GetAxis ("Horizontal");

		if (useSmoke) {
			if (Input.GetAxis ("Horizontal") > 0 || HoriTemp > 0) {
				if (speed < smokeTargetSpeed)
					emSmoke.enabled = true;
				else
					emSmoke.enabled = false;
			} else
				emSmoke.enabled = false;
		}
	}


	void LateUpdate()
	{

		// Get car speed
		speed = GetComponent<Rigidbody2D>().velocity.magnitude;
		if (Input.GetAxis ("Horizontal") < 0 || HoriTemp < 0) 
			speed = -speed;
	}

	// Rotate car on air based on speed
	void Rotate()
	{
		// TODO: Add rotatio ON GROUND & OFF GROUND
		//based on player forward input(Like Hill Climb Racing game)
		if (Input.GetAxis ("Horizontal") > 0 || HoriTemp > 0) {


			GetComponent<Rigidbody2D> ().AddTorque (RotateForce);
		} else {

			if (Input.GetAxis ("Horizontal") < 0.0f || HoriTemp < 0)
				GetComponent<Rigidbody2D> ().AddTorque (-RotateForce);
		}
	}

	// Raycast body to find that car is on the ground or not
	IEnumerator RaycCast()
	{

		while (true) 
		{
			yield return new WaitForEndOfFrame();

			RaycastHit2D hit = Physics2D.Raycast (transform.position, -Vector2.up, 1000);

			float distance = Mathf.Abs(hit.point.y - transform.position.y);


			if (distance < groundDistance)
				isGrounded = true;
			else
				isGrounded = false;

			canRotate = !isGrounded;  	

		}

	}

	// Engine sound system

	public float Multiplyer = 3f;
	public float minP = 1f;
	public float maxP = 2.4f;
	float HoriTemp;

	public void  EngineSoundMin ()
	{
		if (EngineSoundS.pitch > minP)
			EngineSoundS.pitch -= 1.4f * Time.deltaTime;
	}

	public void EngineSoundMobile ()
	{
		
		if (speed < 40) 
		{
			EngineSoundS.pitch = Mathf.Lerp (EngineSoundS.pitch, Mathf.Clamp (HoriTemp * Multiplyer, minP, maxP), Time.deltaTime * 5);
		} 
		else 
		{
			EngineSoundS.pitch = Mathf.Lerp (EngineSoundS.pitch, Mathf.Clamp (HoriTemp * Multiplyer, minP, maxP), Time.deltaTime * 5);
		}
	}

	public void EngineSoundEditor ()
	{

		if (speed < 40) 
		{
			EngineSoundS.pitch = Mathf.Lerp (EngineSoundS.pitch, Mathf.Clamp (Input.GetAxis ("Horizontal") * Multiplyer, minP, maxP), Time.deltaTime * 5);
		} 
		else 
		{
			EngineSoundS.pitch = Mathf.Lerp (EngineSoundS.pitch, Mathf.Clamp (Input.GetAxis ("Horizontal") * Multiplyer, minP, maxP), Time.deltaTime * 5);
		}
	}

	// Vehicle input system
	//this is public function for car Acceleration UI Button
	public void Acceleration ()
	{
		HoriTemp = 1f;
	}
	//this is public function for car Brake\Backward UI Button
	public void Brake ()
	{
		HoriTemp = -1f;
	}


	//this is for when player both release Brake or Acceleration button
	public void GasBrakeRelease ()
	{

		HoriTemp = 0;


	}
}