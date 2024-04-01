using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum Upgrades
{
    Fuel,
    Intake,
    Turbo,
    Engine,
}

public class EngineShop : MonoBehaviour
{
    public CarsManager carsManager;
    public BuyButton[] BuyButtons;
    public GameObject shopOffer;
    public Text CoinsTXT;
    public int currentPage = 0;

    int BUTTONS_IN_SHOP;
    int MAX_UPGRADE;

    void Initiate()
    {
        BUTTONS_IN_SHOP = BuyButtons.Length;
        Car car = carsManager.GetCurrentCar();
        MAX_UPGRADE = car.UpgradePrices.Length;
        UpdateButtons();
    }

    private void Start()
    {
        Initiate();
    }

    private void OnEnable()
    {
        Initiate();
    }

    public void BuyUpgrade(string carItem)
    {
        Car car = carsManager.GetCurrentCar();
        float price = car.UpgradePrices[car.GetIntSavedValue(carItem)];

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= price)
        {

            int priceAsInt = (int)price; // Note: This truncates the decimal part. Consider Mathf.RoundToInt(price) if rounding is preferred.
            int newCoinAmount = currentCoins - priceAsInt;
            CoinsTXT.text = newCoinAmount.ToString();

            // Update the player's coins in PlayerPrefs
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, newCoinAmount);
            PlayerPrefs.Save();

            int selectedValue = car.GetIntSavedValue(carItem);
            car.AddItem(carItem, selectedValue + 1);
            car.SelectItem(carItem, selectedValue + 1);
            UpdateButtons();
        }
        else
        {
            Debug.Log("Not enough coins to buy upgrade: " + carItem);
            // Optionally, show a message or open the shop offer if the player doesn't have enough coins
            shopOffer.SetActive(true);
        }
    }

    // Intermediary methods
    public void BuyIntakeUpgrade()
    {
        BuyUpgrade(CarItemsPrefKeys.Intake);
    }

    public void BuyEngineUpgrade()
    {
        BuyUpgrade(CarItemsPrefKeys.Engine);
    }

    public void BuyTurboUpgrade()
    {
        BuyUpgrade(CarItemsPrefKeys.Turbo);
    }

    public void BuyFuelUpgrade()
    {
        BuyUpgrade(CarItemsPrefKeys.FuelTank);
    }

    public void UpdateButtons()
    {
        Car currentCar = carsManager.GetCurrentCar();
        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);

        for (int i = 0; i < BuyButtons.Length; i++)
        {
            string upgradeKey = GetUpgradeKeyFromIndex(i);
            int currentLevel = currentCar.GetIntSavedValue(upgradeKey);
            bool isMaxLevelReached = currentLevel >= MAX_UPGRADE -  1; // -1 because array indices start at 0
            float priceForNextLevel = currentCar.GetNextUpgradePrice(upgradeKey);

            BuyButton button = BuyButtons[i];
            button.gameObject.SetActive(true); // Always show the button but disable interaction based on conditions
            button.PriceText.text = priceForNextLevel.ToString("F0"); // Assuming PriceText is for showing the price

            if (isMaxLevelReached)
            {
                button.PriceGameObject.SetActive(false);
                button.GetComponent<Button>().interactable = false;
                button.LockGameObject.SetActive(false); // Assuming BuyButton has a public GameObject LockIcon
                button.BottomText.text = "Max Level Reached"; // Change text for max level
            }
            else if (currentCoins < priceForNextLevel)
            {
                button.PriceGameObject.SetActive(true);
                button.GetComponent<Button>().interactable = false; 
                button.LockGameObject.SetActive(true); // Show lock icon when not enough coins
                button.BottomText.text = "Not enough coins";
            }
            else
            {
                button.PriceGameObject.SetActive(true);
                button.GetComponent<Button>().interactable = true;
                button.LockGameObject.SetActive(false); // Hide lock icon when conditions are met
                button.BottomText.text = $"{currentLevel} / {MAX_UPGRADE - 1}"; // Adjusting for human-readable level (1-based)
            }
        }
    }


    // Example implementation for GetUpgradeKeyFromIndex
    private string GetUpgradeKeyFromIndex(int index)
    {
        switch (index)
        {
            case 0: return CarItemsPrefKeys.FuelTank;
            case 1: return CarItemsPrefKeys.Intake;
            case 2: return CarItemsPrefKeys.Turbo;
            case 3: return CarItemsPrefKeys.Engine;
            // Add more cases as needed for your upgrades
            default: return null;
        }
    }

}
