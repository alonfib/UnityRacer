using UnityEngine;

public class CarInitializer : MonoBehaviour
{
	public GameObject[] cars;

	void Start()
	{
		Instantiate(cars[PlayerPrefs.GetInt("SelectedCar")], transform.position, transform.rotation);
	}
}
