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
    public Text CurrentCoinsText;

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
        CurrentCoinsText.text = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins).ToString();
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

            Car car = carsManager.AllCarsPrefabs[carsManager.currentCarIndex];
            PriceText.text = car.Price.ToString();
        }
    }

    //public


    public void BuyCar()
    {
        carsManager.BuyCar();
        UpdateScreen();
        CurrentCoinsText.text = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins).ToString();
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
