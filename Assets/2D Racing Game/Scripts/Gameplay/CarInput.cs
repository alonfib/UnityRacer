using UnityEngine;
using System.Collections;

public class CarInput : MonoBehaviour {


	[HideInInspector]public CarController carController;

	void Start()
	{
		// Start the initialization coroutine
		StartCoroutine(DelayedInitialize());
	}

	IEnumerator DelayedInitialize()
	{
		// Wait for the next frame to ensure all objects are loaded
		yield return null;
		carController = GameObject.FindObjectOfType<CarController>();
		// Further initialization here
	}

	public void Gas ()
	{
		carController.Acceleration ();
	}

	public void Brake ()
	{
		carController.Brake ();
	}

	public void ReleaseGasBrake ()
	{
		carController.GasBrakeRelease ();
	}
}
