using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GarageMenu: MonoBehaviour
{
    public CarsManager carsManager;
    //public GameObject CarModel;

    public GameObject PrevButton;
    public GameObject NextButton;

    public GameObject LockedScreen;

    public Button UpgradeButton;
    public GameObject SelectButton;
    public GameObject BuyButton;

    public Text PriceText;

    void UpdateButtons()
    {
        if(PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCarIndex) == 0)
        {
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
        UpdateScreen();
        PlayerPrefs.SetInt("InitialCarIndex", carsManager.currentCarIndex);

    }


    private void OnDisable()
    {
        if (!carsManager.IsCarOwned(carsManager.currentCarIndex))
        {
            int carIndex = PlayerPrefs.GetInt("InitialCarIndex");
            carsManager.SelectCar(carIndex);
            UpdateScreen();
        }
    }
 
    void UpdateScreen()
    {
        UpdateButtons();
        bool isOwned = carsManager.IsCarOwned(carsManager.currentCarIndex);
        if(isOwned)
        {
            SelectButton.SetActive(true);
            LockedScreen.SetActive(false);
            BuyButton.SetActive(false);
            UpgradeButton.interactable = true;
        } else
        {
            SelectButton.SetActive(false);
            LockedScreen.SetActive(true);
            BuyButton.SetActive(true);
            UpgradeButton.interactable = false;
            PriceText.text = carsManager.currentCar.Price.ToString();
        }
    }

    //public


    public void BuyCar()
    {
        carsManager.BuyCar();
        UpdateScreen();
    }

    public void SelectCar()
    {
        carsManager.SaveSelectedCar();
    }

    public void NextCar()
    {
        carsManager.SelectCar(carsManager.currentCarIndex + 1);
        UpdateScreen();
    }

    public void PervCar()
    {
        carsManager.SelectCar(carsManager.currentCarIndex - 1);
        UpdateScreen();
    }
}
