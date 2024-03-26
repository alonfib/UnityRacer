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
        //ownedWheels = new ;
        BUTTONS_IN_SHOP = BuyButtons.Length;
        UpdateShopPage();
    }

    private void OnEnable()
    {
        UpdateShopPage();

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
        Wheel wheel = GetWheelByIndex(wheelIndex);
        if (carsManager.IsItemOwned(CarItemsPrefKeys.Wheels, wheel.ID))
        {
            Debug.Log("Wheel already owned: " + wheel.ID);
            carsManager.currentWheelIndex = wheelIndex;
            //UpdateShopPage();
            carsManager.UpdateWheels(wheelIndex);
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
            carsManager.UpdateWheels(wheelIndex);
            UpdateShopPage();
        }
        else
        {
            Debug.Log("Not enough coins to buy wheel: " + wheel.ID);
            // Optionally, show a message or open the shop offer if the player doesn't have enough coins
            shopOffer.SetActive(true);
        }
    }

    //Helper method to get a Wheel object by its index
    public Wheel GetWheelByIndex(int wheelIndex)
    {
        // Assuming you have a similar Wheel component as the Car component on the wheel prefabs
        GameObject wheelPrefab = carsManager.WheelsPrefabs[wheelIndex]; // You need to define this array similar to CarsPrefabs
        Wheel wheel = wheelPrefab.GetComponent<Wheel>();
        return wheel;
    }


    void UpdateShopPage()
    {
        //carsManager.UpdateCarView(carsManager.currentCarIndex);
        for (int i = 0; i < BUTTONS_IN_SHOP; i++)
        {
            int wheelIndex = (BUTTONS_IN_SHOP * currentPage) + i;
            if (wheelIndex < carsManager.wheelsImages.Length)
            {
                RawImage image = BuyButtons[i].GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    image.texture = carsManager.wheelsImages[wheelIndex];
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
                    Wheel wheel = GetWheelByIndex(wheelIndex);
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
