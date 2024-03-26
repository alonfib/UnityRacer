using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CarsManager : MonoBehaviour
{
    public Sprite[] carsIcons;
    public Texture2D[] carsImages;
    public Texture2D[] wheelsImages;
    public GameObject[] CarsPrefabs;
    public GameObject[] WheelsPrefabs;

    public int[] carPrices;
    public int currentWheelIndex = 0;

    public GameObject shopOffer;
    public Car currentCar;
    public GameObject CarModel;

    string[] ownedCars;
    string[] ownedCarsIds;
    public int currentCarIndex;

    void Start()
    {
        GetOwnedCars();
    }

    void GetCurrentCar()
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
                currentCar = carComponent;
            }
            else
            {
                // Log an error if the GameObject found doesn't have a Car component
                Debug.LogError("The GameObject tagged as 'Car' does not have a Car component attached.");
            }
        }
        else
        {
            // Log an error if no GameObject with the tag "Car" was found
            Debug.LogError("No GameObject with the tag 'Car' was found.");
        }
    }


    public void UpdateWheels(int wheelIndex)
    {
        currentWheelIndex = wheelIndex;

        // Check if the wheelIndex is valid
        if (wheelIndex < 0 || wheelIndex >= wheelsImages.Length)
        {
            Debug.LogError("Wheel index is out of range.");
            return;
        }

        // Convert the Texture2D to a Sprite
        Texture2D wheelTexture = wheelsImages[wheelIndex];
        Rect rect = new Rect(0, 0, wheelTexture.width, wheelTexture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f); // Center pivot
        Sprite newSprite = Sprite.Create(wheelTexture, rect, pivot);

        Vector2 baselineSize = new Vector2(1.1f, 1.1f); // Example baseline size; adjust this based on your needs
        if(!currentCar)
        {
            GetCurrentCar();
        }

        UpdateWheelSprite(currentCar.FrontWheel, newSprite, baselineSize);
        UpdateWheelSprite(currentCar.RearWheel, newSprite, baselineSize);
    }

    private void UpdateWheelSprite(GameObject wheelGameObject, Sprite newSprite, Vector2 baselineSize)
    {
        if (wheelGameObject != null)
        {
            SpriteRenderer wheelRenderer = wheelGameObject.GetComponent<SpriteRenderer>();
            if (wheelRenderer != null)
            {
                // Apply the new sprite
                wheelRenderer.sprite = newSprite;

                // Reset the scale to 1,1,1 before applying new scale adjustments
                wheelGameObject.transform.localScale = Vector3.one;

                // Calculate the scale factors needed to adjust the new sprite to match the baseline size
                float widthScale = baselineSize.x / newSprite.bounds.size.x;
                float heightScale = baselineSize.y / newSprite.bounds.size.y;

                // Apply the calculated scale to the wheel GameObject
                wheelGameObject.transform.localScale = new Vector3(widthScale, heightScale, 1);

                CircleCollider2D wheelCollider = wheelGameObject.GetComponent<CircleCollider2D>();
                if (wheelCollider != null)
                {
                    // Calculate the radius for the CircleCollider2D based on the sprite size.
                    // The radius is half the sprite's width or height (whichever is larger to ensure the collider fully covers the sprite) in world units.
                    // This calculation uses the sprite's pixelsPerUnit to convert from pixel dimensions to world units.
                    float colliderRadius = Mathf.Max(newSprite.texture.width, newSprite.texture.height) / (2f * newSprite.pixelsPerUnit);

                    // Set the CircleCollider2D's radius to match the calculated radius.
                    wheelCollider.radius = colliderRadius;

                    // Reset the local scale of the wheel GameObject to 1,1,1 to ensure the collider matches the sprite size exactly.
                    //wheelGameObject.transform.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogError("CircleCollider2D not found on wheel GameObject.");
                }
            }
            else
            {
                Debug.LogError("SpriteRenderer not found on wheel GameObject.");
            }
        }
        else
        {
            Debug.LogError("Wheel GameObject is not assigned.");
        }
    }

    public void GetOwnedCars()
    {
        ownedCars = GetOwnedItemsIds(PlayerPrefsKeys.OwnedCarsIds);
    }

    private void AddToOwnedCarsIds(string carId)
    {
        var carIdList = new List<string>(ownedCars) { carId };
        ownedCars = carIdList.ToArray();

        // Convert the ownedCars array back to a comma-separated string and save it to PlayerPrefs
        string updatedCarsIds = string.Join(",", ownedCars);
        PlayerPrefs.SetString(PlayerPrefsKeys.OwnedCarsIds, updatedCarsIds);
        PlayerPrefs.Save();
    }

    private string[] GetOwnedItemsIds(string prefKey)
    {
        string savedIds = PlayerPrefs.GetString(prefKey);
        if (!string.IsNullOrEmpty(savedIds))
        {
            string[] ids = savedIds.Split(',');
            string[] formattedIds = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                formattedIds[i] = ids[i];
            }
            return formattedIds;
        }
        else
        {
            return new string[0];
        }
    }

    public void BuyCar(int carIndex, System.Action onPurchase)
    {
        if(IsCarOwned(carIndex))
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

    public void AddItemToCar(string carPrefKey, string[] formattedItems)
    {
        Car car = GetCarByIndex(currentCarIndex);
        string updatedItems = string.Join(",", formattedItems);
        string formattedKey = car.ID + carPrefKey;

        PlayerPrefs.SetString(formattedKey, updatedItems);
        PlayerPrefs.Save();
    }

    public void UpdateCurrentCarItem(string carItemPrefKey, string itemID)
    {
        Car car = GetCarByIndex(currentCarIndex); // Assuming GetCarByIndex is implemented.
        PlayerPrefs.SetString(string.Concat(CarItemsPrefKeys.Selected, car.ID, carItemPrefKey), itemID);
        PlayerPrefs.Save(); // Make sure to save changes
    }

    public void UpdateCurrentCarItem(string carItemPrefKey, int itemID)
    {
        UpdateCurrentCarItem(carItemPrefKey, itemID.ToString()); // Convert int to string and call the string version
    }

    public string GetCurrentSelectedItem(string carItemPrefKey)
    {
        Car car = GetCarByIndex(currentCarIndex); // Assuming GetCarByIndex is implemented correctly
        if (car == null)
        {
            Debug.LogError("Car not found at current index: " + currentCarIndex);
            return null; // or return string.Empty depending on your error handling preference
        }

        string fullPrefKey = CarItemsPrefKeys.Selected + car.ID + carItemPrefKey;
        return PlayerPrefs.GetString(fullPrefKey, string.Empty); // Returns the item ID or an empty string if not found
    }


    public bool IsItemOwned(string carItemPrefKey, string itemID)
    {
        Car car = GetCarByIndex(currentCarIndex); // This assumes you're checking for an item related to a specific car
        string formattedKey = car.ID + carItemPrefKey; // This will create a unique key for each car and item type (e.g., "Car01Wheels")
        string[] ownedItemIDs = GetOwnedItemsIds(formattedKey);
        bool isOwned = ownedItemIDs.Contains(itemID);

        return isOwned;
    }
 
    public bool IsCarOwned(int carIndex)
    {
        // Check if the carId is within the range of available cars
        if (carIndex < 0 || carIndex >= CarsPrefabs.Length)
        {
            Debug.LogError("Car ID is out of range: " + carIndex);
            // true for BuyCar function validation
            return true;
        }

        // Check if the carId is in the ownedCars array
        Car car = GetCarByIndex(carIndex);
        if (System.Array.IndexOf(ownedCars, car.ID) != -1)
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

    public void UpdateCarView(int carIndex)
    {
        if (carIndex < 0 || carIndex >= CarsPrefabs.Length)
        {
            Debug.LogError("Car index is out of range.");
            return;
        }

        if (carIndex == currentCarIndex)
        {
            Debug.LogError("Car is selected.");
            return;
        }

        // Destroy all children of CarModel
        foreach (Transform child in CarModel.transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the selected car prefab and set it as a child of CarModel
        GameObject selectedCarPrefab = CarsPrefabs[carIndex];
        if (selectedCarPrefab != null)
        {
            GameObject carInstance = Instantiate(selectedCarPrefab, CarModel.transform);
            carInstance.transform.localPosition = Vector3.zero; // Adjust position as needed
            carInstance.transform.localRotation = Quaternion.identity; // Adjust rotation as needed
                                                                       // Optionally, you might want to reset or adjust the scale if necessary
            carInstance.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogError("Selected car prefab is null.");
        }

        // Update other components or UI elements if necessary
    }


    public void SelectCar(int carIndex) 
    {
        PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCarIndex, carIndex);
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