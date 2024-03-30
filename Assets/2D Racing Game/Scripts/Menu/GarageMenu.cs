using UnityEngine;
using System.Collections;

public class GarageMenu: MonoBehaviour
{
    public CarsManager carsManager;
    //public GameObject CarModel;

    public GameObject PrevButton;
    public GameObject NextButton;

    void UpdateButtons()
    {
        if(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCarIndex) == 0)
        {
            Debug.Log(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCarIndex));
            PrevButton.SetActive(false);
        }
        else
        {
            PrevButton.SetActive(true);
        }

        if (carsManager.currentCarIndex == carsManager.AllCarsPrefabs.Length)
        {
            NextButton.SetActive(false);
        }
        else
        {
            NextButton.SetActive(true);
        }
    }

    // Use this for initialization
    void Start()
    {
        UpdateButtons();
    }

    public void NextCar()
    {
        carsManager.SelectCar(carsManager.currentCarIndex + 1);
        UpdateButtons();
    }

    public void PervCar()
    {
        carsManager.SelectCar(carsManager.currentCarIndex - 1);
        UpdateButtons();
    }
}
