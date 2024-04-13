using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorShop : MonoBehaviour
{
    public CarsManager carsManager;
    public BuyButton[] BuyButtons;
    public GameObject shopOffer;
    public Text CoinsTXT;

    public int currentPage = 0;
    List<string> ownedWheelsIds;
    public Items items;

    int BUTTONS_IN_SHOP;

    private void Start()
    {
        BUTTONS_IN_SHOP = BuyButtons.Length;
        Initiate();
    }

    void Initiate()
    {
        StartCoroutine(DelayedInitialize());

        currentPage = 0;

        if (ownedWheelsIds == null)
        {
            ownedWheelsIds = new List<string>();
        }
    }


    private void OnEnable()
    {
        Initiate();
    }

    IEnumerator DelayedInitialize()
    {
        // Wait for the next frame to ensure all objects are loaded
        yield return null;
        UpdateShopPage();
        // Further initialization here
    }

    public void NextPage()
    {
        currentPage++;
        UpdateShopPage();
    }

    public void PrevPage()
    {
        currentPage--;
        UpdateShopPage();
    }

    public void BuyWheel(int selector)
    {
        // Check if the wheel is already owned
        int wheelIndex = BUTTONS_IN_SHOP * currentPage + selector;
        Wheel wheel = items.wheels[wheelIndex];

        if (carsManager.IsItemOwned(CarItemsPrefKeys.Wheels, wheel.ID))
        {
            carsManager.SelectWheels(wheelIndex);
            return;
        }
        Debug.Log("Item Not Owned");

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= wheel.Price)
        {
            int formattedCoins = currentCoins - wheel.Price;

            CoinsTXT.text = formattedCoins.ToString();

            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, formattedCoins);
            carsManager.AddItemToCar(CarItemsPrefKeys.Wheels, wheel.ID);
            carsManager.SelectWheels(wheelIndex);
            UpdateShopPage();
        }
        else
        {
            Debug.Log("Not enough coins to buy wheel: " + wheel.ID);
            // Optionally, show a message or open the shop offer if the player doesn't have enough coins
            shopOffer.SetActive(true);
        }
    }

    void UpdateShopPage()
    {
        for (int i = 0; i < BUTTONS_IN_SHOP; i++)
        {
            int wheelIndex = (BUTTONS_IN_SHOP * currentPage) + i;
            if (wheelIndex < items.wheels.Length)
            {
                RawImage image = BuyButtons[i].Image;
                if (image != null)
                {
                    image.texture = items.wheels[wheelIndex].texture2D;
                    BuyButtons[i].gameObject.SetActive(true);
                }

                Wheel wheel = items.wheels[wheelIndex];
                bool isOwned = carsManager.IsItemOwned(CarItemsPrefKeys.Wheels, wheel.ID);

                BuyButtons[i].PriceGameObject.SetActive(!isOwned);
                BuyButtons[i].LockGameObject.SetActive(!isOwned);
            }
            else
            {
                // If there's no wheel for this button, hide the button
                BuyButtons[i].gameObject.SetActive(false);
            }
        }
    }

}
