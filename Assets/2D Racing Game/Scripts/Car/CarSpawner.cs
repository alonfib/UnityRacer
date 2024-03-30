using UnityEngine;

public class CarInitializer : MonoBehaviour
{
	public GameObject[] cars;

	void Start()
	{
		Instantiate(cars[PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCarIndex)], transform.position, transform.rotation);
	}
}
