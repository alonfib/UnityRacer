using UnityEngine;
using UnityEngine.UI;

public class BodyShop : MonoBehaviour
{
    public CarsManager carsManager;
    public Text CoinsTXT;
    public GameObject shopOffer;
    public BuyButton[] BuyButtons;

    public string ShopCarPrefKey = "";

    void Start()
    {
        UpdateButtons();
    }

    //public void BuyUpgrade(int buttonIndex, string carPrefKey)
    public void BuyUpgrade(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= BuyButtons.Length)
        {
            Debug.LogError("Invalid button index.");
            return;
        }

        BuyButton button = BuyButtons[buttonIndex];
        int price = button.Price;
        string carItem = button.CarItem; // Use CarItem property to determine which item is being bought
        Car currentCar = carsManager.GetCurrentCar();

        if (currentCar.IsItemOwned(ShopCarPrefKey, carItem))
        {
            currentCar.SelectItem(ShopCarPrefKey, carItem);
            currentCar.LoadBodyUpgrades();
            UpdateButtons();
            return;
        }
            int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= price)
        {
            // Deduct the price from the player's coins and update the UI
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - price);
            CoinsTXT.text = (currentCoins - price).ToString();

            // Update the car with the purchased item
            currentCar.AddItem(ShopCarPrefKey, carItem);
            currentCar.SelectItem(ShopCarPrefKey, carItem);
            currentCar.LoadBodyUpgrades();
            // Refresh UI buttons to reflect changes
            UpdateButtons();
        }
        else
        {
            Debug.Log($"Not enough coins to buy {carItem}.");
            shopOffer.SetActive(true);
        }
    }


    public void UpdateButtons()
    {
        Car currentCar = carsManager.GetCurrentCar();

        foreach (BuyButton button in BuyButtons)
        {
            string carItem = button.CarItem;
            bool isOwned = currentCar.IsItemOwned(ShopCarPrefKey, carItem) ;
            bool isSelected = carItem == currentCar.GetSelectedItemId(ShopCarPrefKey);

            button.LockGameObject.SetActive(!isOwned);
            button.PriceGameObject.SetActive(!isOwned && !isSelected);
            button.PriceText.text = $"{button.Price} Coins";

            if (isSelected)
            {
                button.PriceGameObject.SetActive(false);
                button.BottomText.text = "Selected";
                button.BottomText.color = Color.green;
            }
            else if (isOwned)
            {
                button.PriceGameObject.SetActive(false);
                button.BottomText.text = "Owned";
                button.BottomText.color = Color.white;
            }
            else
            {
                button.PriceGameObject.SetActive(true);
                button.PriceText.text = button.Price.ToString();
                button.BottomText.text = "Buy";
                button.BottomText.color = Color.yellow;
            }
        }
    }

}
