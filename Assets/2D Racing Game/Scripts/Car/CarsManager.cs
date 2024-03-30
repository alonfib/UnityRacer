using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CarsManager : MonoBehaviour
{
    public Sprite[] carsIcons;
    public Texture2D[] carsImages;
    public Sprite[] wheelsSprites;
    public Car[] AllCarsPrefabs;
    public Wheel[] AllWheelsPrefabs;

    public int[] carPrices;
    public int currentWheelIndex = 0;

    public GameObject shopOffer;
    public Car currentCar;
    public GameObject CarModel;

    public List<Car> OwnedCarsPrefabs = new List<Car>();

    List<string> ownedCarsIds;
    public int currentCarIndex;

    // general

    void Start()
    {
        ownedCarsIds = GetOwnedItemsIds(PlayerPrefsKeys.OwnedCarsIds);
        if(ownedCarsIds.ToArray().Length == 0)
        {
            ownedCarsIds.Add(AllCarsPrefabs[0].ID);
        }

        InitializeOwnedCarsPrefabs(ownedCarsIds);
        currentCar = GetCurrentCar();
    }


    public Car GetCurrentCar()
    {
        // Find the GameObject with the tag "Car"
        GameObject carGameObject = GameObject.FindGameObjectWithTag("Car");

        if (carGameObject != null)
        {
            // Try to get the Car component on the found GameObject
            Car carComponent = carGameObject.GetComponent<Car>();

            if (carComponent != null)
            {
                // Assign the found Car component to currentCar
                return carComponent;
            }
            else
            {
                return null;
                // Log an error if the GameObject found doesn't have a Car component
                Debug.LogError("The GameObject tagged as 'Car' does not have a Car component attached.");
            }
        }
        else
        {
            return null;
            // Log an error if no GameObject with the tag "Car" was found
            Debug.LogError("No GameObject with the tag 'Car' was found.");
        }
    }


    void InitializeOwnedCarsPrefabs(List<string> carIds)
    {
        OwnedCarsPrefabs.Clear();

        foreach (string carId in carIds)
        {
            Car carPrefab = GetCarById(carId);
            if (carPrefab != null)
            {
                OwnedCarsPrefabs.Add(carPrefab);
            }
            else
            {
                // Log an error if no matching prefab is found for an ID.
                Debug.LogError($"No matching Car prefab found for ID: {carId}");
            }
        }
    }

    Car GetCarByIndex(int carIndex)
    {
        Car car = AllCarsPrefabs[carIndex];
        return car;
    }

    List<string> GetOwnedItemsIds(string prefKey)
    {
        string savedIds = PlayerPrefs.GetString(prefKey);
        if (!string.IsNullOrEmpty(savedIds))
        {
            string[] ids = savedIds.Split(',');
            List<string> formattedIds = new List<string>();
            for (int i = 0; i < ids.Length; i++)
            {
                formattedIds.Add(ids[i]);
            }
            return formattedIds;
        }
        else
        {
            return new List<string>();
        }
    }

    public Car GetCarById(string carId)
    {
        foreach (Car carPrefab in AllCarsPrefabs)
        {
            //Car carComponent = carPrefab.GetComponent<Car>();
            if (carPrefab != null && carPrefab.ID == carId)
            {
                return carPrefab;
            }
        }

        Debug.LogError($"No car prefab found with ID: {carId}");
        return null; // Return null if no matching car is found
    }

    public void SelectCar(int carIndex)
    {
        currentCarIndex = carIndex;
        PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCarIndex, carIndex);
        UpdateCarView(carIndex);
        currentCar = GetCurrentCar();
    }

    public void SelectCarById(string carId)
    {
        // Find the Car object by its ID.
        Car carToSelect = GetCarById(carId);

        if (carToSelect != null)
        {
            // Set the currentCar to the found Car object.
            currentCar = carToSelect;

            // Find the index of the Car object in the AllCarsPrefabs array.
            int index = System.Array.IndexOf(AllCarsPrefabs, carToSelect);

            if (index != -1)
            {
                // Update the currentCarIndex to the index of the found Car.
                currentCarIndex = index;
                PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCarIndex, index);
                PlayerPrefs.Save();

                // Update the car view to reflect the change.
                UpdateCarView(index);
            }
            else
            {
                Debug.LogError($"Car with ID: {carId} was found, but it's not in the AllCarsPrefabs array.");
            }
        }
        else
        {
            Debug.LogError($"No car found with ID: {carId}.");
        }
    }

    // car shop functions

    public void BuyCar(int carIndex, System.Action onPurchase)
    {
        if (IsCarOwned(carIndex))
        {
            UpdateCarView(carIndex);
        }

        Car car = GetCarByIndex(carIndex);

        int currentCoins = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins);
        if (currentCoins >= car.Price)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, currentCoins - car.Price);
            AddToOwnedCarsIds(car.ID);


            // Optionally, update UI or give feedback to the player
            Debug.Log("Car purchased: " + carIndex);
            onPurchase();
        }
        else
            shopOffer.SetActive(true);
    }

    public bool IsCarOwned(int carIndex)
    {
        // Check if the carId is within the range of available cars
        if (carIndex < 0 || carIndex >= AllCarsPrefabs.Length)
        {
            Debug.LogError("Car ID is out of range: " + carIndex);
            // true for BuyCar function validation
            return true;
        }

        // Check if the carId is in the ownedCars array
        Car car = GetCarByIndex(carIndex);
        if (System.Array.IndexOf(ownedCarsIds.ToArray(), car.ID) != -1)
        {
            return true; // The car is owned
        }

        return false; // The car is not owned
    }

    void AddToOwnedCarsIds(string carId)
    {
        ownedCarsIds.Add(carId);

        // Convert the ownedCars array back to a comma-separated string and save it to PlayerPrefs
        string updatedCarsIds = string.Join(",", ownedCarsIds);
        PlayerPrefs.SetString(PlayerPrefsKeys.OwnedCarsIds, updatedCarsIds);
        PlayerPrefs.Save();
    }

    // current car functions

    public void UpdateCarView(int carIndex)
    {
        if (carIndex < 0 || carIndex >= AllCarsPrefabs.Length)
        {
            Debug.LogError("Car index is out of range.");
            return;
        }

        // Destroy all children of CarModel
        foreach (Transform child in CarModel.transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the selected car prefab and set it as a child of CarModel
        GameObject selectedCarPrefab = AllCarsPrefabs[carIndex].gameObject;
        if (selectedCarPrefab != null)
        {
            GameObject carInstance = Instantiate(selectedCarPrefab, CarModel.transform);
            carInstance.transform.localPosition = Vector3.zero; // Adjust position as needed
            carInstance.transform.localRotation = Quaternion.identity; // Adjust rotation as needed
                                                                       // Optionally, you might want to reset or adjust the scale if necessary
            carInstance.transform.localScale = Vector3.one;
            currentCar = GetCurrentCar();
        }
        else
        {
            Debug.LogError("Selected car prefab is null.");
        }

        // Update other components or UI elements if necessary
    }

    public void UpdateCurrentCarItem(string carItemPrefKey, string itemID)
    {
        Car car = GetCurrentCar();
        PlayerPrefs.SetString(string.Concat(CarItemsPrefKeys.Selected, car.ID, carItemPrefKey), itemID);
        PlayerPrefs.Save(); // Make sure to save changes
    }

    // In case itemID is number.
    public void UpdateCurrentCarItem(string carItemPrefKey, int itemID)
    {
        UpdateCurrentCarItem(carItemPrefKey, itemID.ToString()); // Convert int to string and call the string version
    }

    public string GetCurrentSelectedItem(string carItemPrefKey)
    {
         Car car = GetCurrentCar();
        if (car == null)
        {
            Debug.LogError("Car not found at current index: " + currentCarIndex);
            return null; // or return string.Empty depending on your error handling preference
        }

        string fullPrefKey = CarItemsPrefKeys.Selected + car.ID + carItemPrefKey;
        return PlayerPrefs.GetString(fullPrefKey, string.Empty); // Returns the item ID or an empty string if not found
    }

    public void UpdateCurrentWheels(int wheelIndex)
    {
        currentWheelIndex = wheelIndex;

        // Check if the wheelIndex is valid
        if (wheelIndex < 0 || wheelIndex >= wheelsSprites.Length)
        {
            Debug.LogError("Wheel index is out of range.");
            return;
        }

        Car currentCar = GetCurrentCar();
        currentCar.UpdateCurrentWheels(wheelsSprites[wheelIndex]);
    }

    public bool IsItemOwned(string carItemPrefKey, string itemID)
    {
        Car car = GetCurrentCar(); // This assumes you're checking for an item related to a specific car
        string formattedKey = car.ID + carItemPrefKey; // This will create a unique key for each car and item type (e.g., "Car01Wheels")
        string[] ownedItemIDs = GetOwnedItemsIds(formattedKey).ToArray();
        bool isOwned = ownedItemIDs.Contains(itemID);

        return isOwned;
    }

    public void AddItemToCar(string carPrefKey, string[] formattedItems)
    {
        Car car = GetCurrentCar();
        string updatedItems = string.Join(",", formattedItems);
        string formattedKey = car.ID + carPrefKey;

        PlayerPrefs.SetString(formattedKey, updatedItems);
        PlayerPrefs.Save();
    }

}   