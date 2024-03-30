using UnityEngine;
using System.Collections;

public class StartPoint : MonoBehaviour {

	public GameObject[] cars;

	void Start () {
		Instantiate (cars [PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedCarIndex)], transform.position, transform.rotation);
	}
}
