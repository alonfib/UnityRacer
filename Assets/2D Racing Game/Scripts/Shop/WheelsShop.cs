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
    string[] ownedWheelsIds;

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
            ownedWheelsIds = new string[0];
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
        Wheel wheel = carsManager.AllWheelsPrefabs[wheelIndex];
        if (carsManager.IsItemOwned(CarItemsPrefKeys.Wheels, wheel.ID))
        {
            Debug.Log("Wheel owned: " + wheel.ID);
            carsManager.currentWheelIndex = wheelIndex;
            carsManager.UpdateCurrentWheels(wheelIndex);
            return; // Early return if the wheel is already owned
        }

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= wheel.Price)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - wheel.Price);
     
        var wheelIdList = new List<string>(ownedWheelsIds) { wheel.ID };
            ownedWheelsIds = wheelIdList.ToArray();

            carsManager.AddItemToCar(CarItemsPrefKeys.Wheels, ownedWheelsIds);
            carsManager.currentWheelIndex = wheelIndex;
            Debug.Log("Wheel purchased: " + wheel.ID);
            carsManager.UpdateCurrentWheels(wheelIndex);
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
        //carsManager.UpdateCarView(carsManager.currentCarIndex);
        for (int i = 0; i < BUTTONS_IN_SHOP; i++)
        {
            int wheelIndex = (BUTTONS_IN_SHOP * currentPage) + i;
            if (wheelIndex < carsManager.wheelsSprites.Length)
            {
                RawImage image = BuyButtons[i].GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    image.texture = carsManager.wheelsSprites[wheelIndex].texture;
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
                    Wheel wheel = carsManager.AllWheelsPrefabs[wheelIndex];
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
