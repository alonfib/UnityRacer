using UnityEngine;
using System.Collections;

public class NewCarInput : MonoBehaviour {
	[HideInInspector]public NewCarController carController;

	IEnumerator Start ()
	{
		yield return new WaitForSeconds (.5f);
		carController = GameObject.FindObjectOfType<NewCarController> ();
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
