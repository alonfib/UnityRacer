using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarsManager : MonoBehaviour
{
    public Sprite[] carsIcons;
    public Texture2D[] carsImages;
    public Texture2D[] wheelsImages;
    public GameObject[] CarsPrefabs;
    public int[] carPrices;

    public GameObject shopOffer;


    int[] ownedCars;

    void Start()
    {
        GetOwnedCars();
    }

    public void GetOwnedCars()
    {
        string prefsCarsIds = PlayerPrefs.GetString(PlayerPrefsKeys.OwnedCarsIds);
        if (!string.IsNullOrEmpty(prefsCarsIds))
        {
            string[] carIds = prefsCarsIds.Split(',');
            ownedCars = new int[carIds.Length];
            for (int i = 0; i < carIds.Length; i++)
            {
                if (int.TryParse(carIds[i], out int carId))
                {
                    ownedCars[i] = carId;
                }
                else
                {
                    Debug.LogError("Invalid car ID in PlayerPrefs: " + carIds[i]);
                }
            }
        }
        else
        {
            ownedCars = new int[0];
        }
    }

    public void BuyCar(int carId, System.Action onPurchase)
    {
        if(IsCarOwned(carId))
        {
            return;
        }

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= carPrices[carId])
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - carPrices[carId]);
            var carIdList = new List<int>(ownedCars) { carId };
            ownedCars = carIdList.ToArray();

            // Convert the ownedCars array back to a comma-separated string and save it to PlayerPrefs
            string updatedCarsIds = string.Join(",", ownedCars);
            PlayerPrefs.SetString(PlayerPrefsKeys.OwnedCarsIds, updatedCarsIds);
            PlayerPrefs.Save();

            // Optionally, update UI or give feedback to the player
            Debug.Log("Car purchased: " + carId);
            onPurchase();
        }
        else
            shopOffer.SetActive(true);
    }

    public bool IsCarOwned(int carId)
    {
        // Check if the carId is within the range of available cars
        if (carId < 0 || carId >= CarsPrefabs.Length)
        {
            Debug.LogError("Car ID is out of range: " + carId);
            // true for BuyCar function validation
            return true;
        }

        // Check if the carId is in the ownedCars array
        if (System.Array.IndexOf(ownedCars, carId) != -1)
        {
            return true; // The car is owned
        }

        return false; // The car is not owned
    }
}   