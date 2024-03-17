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

    string[] ownedWheels;
    string[] ownedCars;
    string[] ownedCarsIds;

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
            ownedCars = new string[carIds.Length];
            for (int i = 0; i < carIds.Length; i++)
            {
                    ownedCars[i] = carIds[i];
                {
                    Debug.LogError("Invalid car ID in PlayerPrefs: " + carIds[i]);
                }
            }
        }
        else
        {
            ownedCars = new string[0];
        }
    }

    public void BuyCar(int carIndex, System.Action onPurchase)
    {
        if(IsCarOwned(carIndex))
        {
            return;
        }

        Car car = GetCarByIndex(carIndex);

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= car.Price)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - car.Price);
            var carIdList = new List<string>(ownedCars) { car.ID };
            ownedCars = carIdList.ToArray();

            // Convert the ownedCars array back to a comma-separated string and save it to PlayerPrefs
            string updatedCarsIds = string.Join(",", ownedCars);
            PlayerPrefs.SetString(PlayerPrefsKeys.OwnedCarsIds, updatedCarsIds);
            PlayerPrefs.Save();

            // Optionally, update UI or give feedback to the player
            Debug.Log("Car purchased: " + carIndex);
            onPurchase();
        }
        else
            shopOffer.SetActive(true);
    }

    public void BuyWheel(int wheelIndex)
    {

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

    Car GetCarByIndex(int carIndex)
    {
        GameObject carPrefab = CarsPrefabs[carIndex];
        Car car = carPrefab.GetComponent<Car>();
        return car;
    }

    public void UpdateCarView(int carId)
    {
        GameObject carView = GameObject.FindGameObjectWithTag("CarView");
        if (carView == null)
        {
            Debug.LogError("CarView GameObject not found.");
            return;
        }

        // Destroy all children of CarView
        foreach (Transform child in carView.transform)
        {
            Destroy(child.gameObject);
        }

    

        GameObject frontWheel = CreateChildGameObject(carView, "FrontWheel", wheelsImages[0]);
        RectTransform frontWheelRect = frontWheel.GetComponent<RectTransform>();
        frontWheelRect.sizeDelta = new Vector2(62, 62); // Adjust as needed
        frontWheelRect.localPosition = new Vector3(118, -48, 0);
            

        GameObject rearWheel = CreateChildGameObject(carView, "RearWheel", wheelsImages[0]);
        RectTransform rearWheelRect = rearWheel.GetComponent<RectTransform>();
        rearWheelRect.sizeDelta = new Vector2(62, 62); // Adjust as needed
        rearWheelRect.localPosition = new Vector3(-127, -48, 0);


        GameObject body = CreateChildGameObject(carView, "CarBody", carsImages[carId]);
        RectTransform bodyRect = body.GetComponent<RectTransform>();
        bodyRect.sizeDelta = new Vector2(380, 120); // Adjust as needed

    }

    // This method now always creates a new GameObject instead of checking if it exists
    GameObject CreateChildGameObject(GameObject parent, string childName, Texture2D texture)
    {
        // Create new GameObject as a child
        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent.transform, false);

        // Add RawImage component and set the texture
        RawImage rawImage = child.AddComponent<RawImage>();
        rawImage.texture = texture;

        return child;
    }

    public int CalculateCurrentMotorPower()
    {
        return 0;
    }

    public int CalculateCurrentMaxSpeed()
    {
        return 0;
    }

    public int CalculateOnAirRotation()
    {
        return 0;
    }

    public int CalculateOffAirRotation()
    {
        return 0;
    }
}   