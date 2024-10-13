using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public static class CarObjectsTags
{
    public const string FrontWheel = "FrontWheel";
    public const string RearWheel = "RearWheel";
    public const string Car = "Car";
    public const string Body = "Player";
    //public const string Body = "Player";
}

public class Car : MonoBehaviour
{
    public string ID;
    public int Price;
    public Texture2D texture2D;

    public GameObject RearWheel;
    public GameObject FrontWheel;
    public CarController carController;

    public float[] exhaustUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] ecuUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] turboUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] fuelUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    public float[] brakesUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };
    //public float[] tiresUpgrades = new float[] { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };

    public float[] UpgradePrices = new float[] { 2000, 5000, 8000, 12000, 15000, 20000, 30000 , 50000, 75000, 100000 };

    int exhaustUpgrade = 0;
    int ecuUpgrade = 0; 
    int turboUpgrade = 0;
    int fuelUpgrade = 0;
    int brakesUpgrade = 0;
    //int tiresUpgrade = 0;

    public Wheel DefaultWheel;

    //string currentTire = "default";
    //string[] tires;

    string currentWheel = "default";
    string[] wheels;

    string currentBodyColor = "default";
    string[] colors;
    Items items;

    DrivetrainType drivetrain = DrivetrainType.RWD;
    string suspentions = SuspentionsTypes.Default;
    string tires = TiresTypes.Default;
    string brakes = BrakesTypes.Default;

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
        ecuUpgrade = GetIntSavedValue(CarItemsPrefKeys.ECU);
        turboUpgrade = GetIntSavedValue(CarItemsPrefKeys.Turbo);
        fuelUpgrade = GetIntSavedValue(CarItemsPrefKeys.FuelTank);
        brakesUpgrade = GetIntSavedValue(CarItemsPrefKeys.Brakes);
        //tiresUpgrade = GetIntSavedValue(CarItemsPrefKeys.Tires);
    }

    void LoadDrivetrain()
    {
        string selectedDrivetrain = GetSelectedItemId(CarItemsPrefKeys.Drivetrain); // Get the saved drivetrain as a string

        // Check if a valid drivetrain string was retrieved
        if (!string.IsNullOrEmpty(selectedDrivetrain))
        {
            DrivetrainType parsedDrivetrain;

            // Try parsing the string into the DrivetrainType enum
            if (Enum.TryParse(selectedDrivetrain, out parsedDrivetrain))
            {
                drivetrain = parsedDrivetrain;
                carController.drivetrain = drivetrain;
                carController.UpdateDriveTrain();
            }
            else
            {
                Debug.LogError("Invalid drivetrain type retrieved from PlayerPrefs: " + selectedDrivetrain);
            }
        }
        else
        {
            Debug.Log("No drivetrain type selected, using default settings.");
        }
    }

    void LoadSuspensions()
    {
        string selectedSuspentions = GetSelectedItemId(CarItemsPrefKeys.Suspension);
        if (!string.IsNullOrEmpty(selectedSuspentions))
        {
            suspentions = selectedSuspentions;

            // Apply the suspension changes to the car. This is conceptual;
            // you need to implement ApplySuspensionType in a way that matches your game's mechanics.
            ApplySuspensionType(suspentions);
        }
        else
        {
            // If no suspension is selected, you could apply a default or keep the current setting.
            Debug.Log("No specific suspension selected, using default or existing settings.");
        }
    }

    void ApplySuspensionType(string suspensionType)
    {
        switch (suspensionType)
        {
            case SuspentionsTypes.Default:
                SetSuspentions(3);
                // Apply default suspension settings
                break;
            case SuspentionsTypes.Sport:
                SetSuspentions(2.7f);

                // Apply medium suspension settings
                break;
            case SuspentionsTypes.Rally:
                SetSuspentions(3.2f);
                // Apply high suspension settings
                break;
            case SuspentionsTypes.High:
                SetSuspentions(4);
                // Apply low suspension settings
                break;
            default:
                Debug.LogError($"Unknown suspension type: {suspensionType}");
                break;
        }
    }

    void SetSuspentions(float height)
    {
        // Get the current suspension settings as a copy
        JointSuspension2D frontSuspension = carController.frontMotorWheel.suspension;
        JointSuspension2D rearSuspension = carController.rearMotorWheel.suspension;

        // Modify the copy
        frontSuspension.frequency = height;
        rearSuspension.frequency = height;

        // Assign the modified copy back to the suspension property
        carController.frontMotorWheel.suspension = frontSuspension;
        carController.rearMotorWheel.suspension = rearSuspension;
    }

    void LoadTires()
    {
        string selectedTires = GetSelectedItemId(CarItemsPrefKeys.Tires);
        if (!string.IsNullOrEmpty(selectedTires))
        {
            tires = selectedTires;

            // Apply the suspension changes to the car. This is conceptual;
            // you need to implement ApplySuspensionType in a way that matches your game's mechanics.
            ApplTireType(tires);
        }
        else
        {
            // If no suspension is selected, you could apply a default or keep the current setting.
            Debug.Log("No specific suspension tires, using default or existing settings.");
        }
    }

    void ApplTireType(string tires)
    {
        switch (tires)
        {
            case TiresTypes.Default:
                SetTiresUpgrade(0.1f);
                // Apply default suspension settings
                break;
            case TiresTypes.Sport:
                SetTiresUpgrade(0.3f);

                // Apply medium suspension settings
                break;
            case TiresTypes.StreetRacing:
                SetTiresUpgrade(6f);
                // Apply high suspension settings
                break;
            case TiresTypes.Racing:
                SetTiresUpgrade(0.8f);
                // Apply low suspension settings
                break;
            default:
                Debug.LogError($"Unknown tires type: {tires}");
                break;
        }
    }

    void SetTiresUpgrade(float dampingRatio)
    {
        JointSuspension2D frontSuspension = carController.frontMotorWheel.suspension;
        JointSuspension2D rearSuspension = carController.rearMotorWheel.suspension;

        // Modify the copy
        frontSuspension.dampingRatio = dampingRatio;
        rearSuspension.dampingRatio = dampingRatio;

        // Assign the modified copy back to the suspension property
        carController.frontMotorWheel.suspension = frontSuspension;
        carController.rearMotorWheel.suspension = rearSuspension;
    }

    void LoddBrakes()
    {
        string selectedBrakes = GetSelectedItemId(CarItemsPrefKeys.Brakes);
        if (!string.IsNullOrEmpty(selectedBrakes))
        {
            brakes = selectedBrakes;

            // Apply the suspension changes to the car. This is conceptual;
            // you need to implement ApplySuspensionType in a way that matches your game's mechanics.
            ApplBrakeType(brakes);
        }
        else
        {
            // If no suspension is selected, you could apply a default or keep the current setting.
            Debug.Log("No specific brakes selected, using default or existing settings.");
        }
    }

    void ApplBrakeType(string brakes)
    {
        switch (brakes)
        {
            case BrakesTypes.Default:
                SetBrakeUpgrade(0.1f);
                // Apply default suspension settings
                break;
            case BrakesTypes.Sport:
                SetBrakeUpgrade(0.3f);

                // Apply medium suspension settings
                break;
            case BrakesTypes.StreetRacing:
                SetBrakeUpgrade(6f);
                // Apply high suspension settings
                break;
            case BrakesTypes.Racing:
                SetBrakeUpgrade(0.8f);
                // Apply low suspension settings
                break;
            default:
                Debug.LogError($"Unknown tires type: {tires}");
                break;
        }
    }


    void SetBrakeUpgrade(float brakePower)
    {
        carController.brakePower = brakePower;
    }


    public void LoadBodyUpgrades()
    {
        LoadDrivetrain();
        LoadSuspensions();
        LoadTires();
        LoddBrakes();
    }

    void initCar()
    {
        items = GameObject.FindGameObjectWithTag("CarItems").GetComponent<Items>();
        InitWheels();
        LoadUpgrades();
        LoadBodyUpgrades();
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
        RearWheel = GameObject.FindGameObjectWithTag(CarObjectsTags.RearWheel);
        FrontWheel = GameObject.FindGameObjectWithTag(CarObjectsTags.FrontWheel);

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
