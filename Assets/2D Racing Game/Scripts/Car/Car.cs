using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Car : MonoBehaviour
{
    public string ID;
    public int Price;
    public Texture2D texture2D;

    public GameObject RearWheel;
    public GameObject FrontWheel;

    public float[] exhaustUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] intakeUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] turboUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] fuelUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] brakesUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] tiresUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };

    public float[] UpgradePrices = new float[] { 2000, 5000, 8000, 12000, 15000, 20000, 30000 , 50000, 75000, 100000 };

    int exhaustUpgrade = 0;
    int intakeUpgrade = 0;
    int turboUpgrade = 0;
    int fuelUpgrade = 0;
    int brakesUpgrade = 0;
    int tiresUpgrade = 0;

    public Wheel DefaultWheel;

    //string currentTire = "default";
    //string[] tires;

    string currentWheel = "default";
    string[] wheels;

    string currentBodyColor = "default";
    string[] colors;
    Items items;

    DrivetrainType drivetrain = DrivetrainType.RWD;
    SuspentionsTypes suspentions = SuspentionsTypes.Default;

    void Awake()
    {
        // Start the initialization coroutine
        StartCoroutine(DelayedInitialize());
    }

    IEnumerator DelayedInitialize()
    {
        // Wait for the next frame to ensure all objects are loaded
        yield return null;
        initCar();
        // Further initialization here
    }

    void LoadUpgrades()
    {
        exhaustUpgrade = GetIntSavedValue(CarItemsPrefKeys.Exhaust);
        intakeUpgrade = GetIntSavedValue(CarItemsPrefKeys.Intake);
        turboUpgrade = GetIntSavedValue(CarItemsPrefKeys.Turbo);
        fuelUpgrade = GetIntSavedValue(CarItemsPrefKeys.FuelTank);
        brakesUpgrade = GetIntSavedValue(CarItemsPrefKeys.Brakes);
        tiresUpgrade = GetIntSavedValue(CarItemsPrefKeys.Tires);
    }

    void initCar()
    {
        items = GameObject.FindGameObjectWithTag("CarItems").GetComponent<Items>();
        InitWheels();
        LoadUpgrades();

    }

    public int GetIntSavedValue(string carPrefKey)
    {
        string selected = GetSelectedItemId(carPrefKey);
        if (!string.IsNullOrEmpty(selected))
        {
            int result;
            if (int.TryParse(selected, out result))
            {
                return result;
            }
            else
            {
                Debug.LogError("Failed to parse selected item ID to int for key: " + carPrefKey);
                return 0; 
            }
        }
        else
        {
            Debug.Log("No selected item ID found for key: " + carPrefKey);
            return 0; 
        }
    }

    private void InitWheels()
    {
        GetWheelsObjects();

        string lastSelectedWheelId = GetSelectedItemId(CarItemsPrefKeys.Wheels);
        if (!string.IsNullOrEmpty(lastSelectedWheelId))
        {
            // Find the corresponding Wheel object by ID
            Wheel lastSelectedWheel = FindWheelById(lastSelectedWheelId);
            if (lastSelectedWheel != null)
            {
                UpdateCurrentWheels(lastSelectedWheel);
            }
        } else
        {
            AddItem(CarItemsPrefKeys.Wheels, DefaultWheel.ID);
            SelectItem(CarItemsPrefKeys.Wheels, DefaultWheel.ID);
            UpdateCurrentWheels(DefaultWheel);
        }
    }

    private Wheel FindWheelById(string wheelId)
    {
        // Assuming you have a way to access all Wheel objects, e.g., through the items variable or directly here
        foreach (var wheel in items.wheels) // items.AllWheels represents all available Wheel objects
        {
            if (wheel.ID == wheelId)
            {
                return wheel;
            }
        }

        return null;
    }

    private void GetWheelsObjects()
    {
        RearWheel = GameObject.FindGameObjectWithTag("RearWheel");
        FrontWheel = GameObject.FindGameObjectWithTag("FrontWheel");

        if (RearWheel == null || FrontWheel == null)
        {
            Debug.LogError("Failed to find one or more wheels. Are they tagged correctly?");
        }
        else
        {
            Debug.Log("Wheels found successfully.");
        }
    }

    public void UpdateCurrentWheels(Wheel wheel)
    {
        GetWheelsObjects();
        Sprite newSprite = wheel.sprite;
        Vector2 baselineSize = new Vector2(1.1f, 1.1f); // adjust this based on your needs
        UpdateWheelSprite(FrontWheel, newSprite, baselineSize);
        UpdateWheelSprite(RearWheel, newSprite, baselineSize);
    }
        
    public List<string> GetOwnedItemsIds(string prefKey)
    {
        string savedIds = PlayerPrefs.GetString(ID + prefKey);
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

    public bool IsItemOwned(string carItemPrefKey, string itemID)
    {
        List<string> ownedItemIDs = GetOwnedItemsIds(carItemPrefKey);
        bool isOwned = ownedItemIDs.Contains(itemID);
        return isOwned;
    }

    public void AddItem(string carPrefKey, string itemId)
    {
        List<string> formattedItems = GetOwnedItemsIds(carPrefKey);
        formattedItems.Add(itemId);
        string updatedItems = string.Join(",", formattedItems);
        string formattedKey = ID + carPrefKey; // This will create a unique key for each car and item type (e.g., "Car01Wheels")
        PlayerPrefs.SetString(formattedKey, updatedItems);
        PlayerPrefs.Save();
    }

    // In case itemID is number.
    public void AddItem(string carPrefKey, int itemId)
    {
        AddItem(carPrefKey, itemId.ToString());
    }

    public void SelectItem(string carPrefKey, string itemId)
    {
        PlayerPrefs.SetString(string.Concat(ID, CarItemsPrefKeys.Selected, carPrefKey), itemId);
        PlayerPrefs.Save(); // Make sure to save changes
    }

    // In case itemID is number.
    public void SelectItem(string carItemPrefKey, int itemID)
    {
        SelectItem(carItemPrefKey, itemID.ToString()); // Convert int to string and call the string version
    }

    public string GetSelectedItemId(string carPrefKey)
    {
        return PlayerPrefs.GetString(string.Concat(ID, CarItemsPrefKeys.Selected, carPrefKey));
    }

    void UpdateWheelSprite(GameObject wheelGameObject, Sprite newSprite, Vector2 baselineSize)
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

    public float GetNextUpgradePrice(string carPrefKey)
    {
        int currentLevel = GetIntSavedValue(carPrefKey);
        if (currentLevel >= 0 && currentLevel < UpgradePrices.Length - 1)
        {
            return UpgradePrices[currentLevel];
        }
        else
        {
            Debug.LogError("Invalid upgrade level or max level reached for: " + carPrefKey);
            return float.MaxValue; // Indicates an error or max level reached
        }
    }
}
