using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WheelsShop : MonoBehaviour
{
    public CarsManager carsManager;
    public GameObject[] BuyButtons;
    public GameObject shopOffer;

    public int currentPage = 0;
    List<string> ownedWheelsIds;
    public Items items;

    int BUTTONS_IN_SHOP;

    private void Start()
    {
        BUTTONS_IN_SHOP = BuyButtons.Length;
    }

    private void OnEnable()
    {
        UpdateShopPage();
        currentPage = 0;

        if (ownedWheelsIds == null)
        {
            ownedWheelsIds = new List<string>();
        }
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

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= wheel.Price)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - wheel.Price);
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
                RawImage image = BuyButtons[i].GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    image.texture = items.wheels[wheelIndex].texture2D;
                    BuyButtons[i].SetActive(true);
                }

                // Corrected approach to find the lock icon by tag
                GameObject lockIcon = null;
                foreach (Transform child in BuyButtons[i].transform)
                {   
                    if (child.tag == "ShopLock")
                    {
                        lockIcon = child.gameObject;
                        break;
                    }
                }

                if (lockIcon != null)
                {
                    // Check if the wheel is owned
                    Wheel wheel = items.wheels[wheelIndex];
                    bool isOwned = carsManager.IsItemOwned(CarItemsPrefKeys.Wheels, wheel.ID);
                    lockIcon.SetActive(!isOwned);
                }
            }
            else
            {
                // If there's no wheel for this button, hide the button
                BuyButtons[i].SetActive(false);
            }
        }
    }

}
