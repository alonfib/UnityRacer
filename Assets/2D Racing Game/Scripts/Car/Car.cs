using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour
{
    public string ID;
    public int Price;
    public Texture2D texture2D;

    public GameObject RearWheel;
    public GameObject FrontWheel;

    int exhaustUpgrade = 0;
    int intakeUpgrade = 0;
    int turboUpgrade = 0;
    int fuelUpgrade = 0;
    int brakesUpgrade = 0;
    int tiresUpgrade = 0;

    //string currentTire = "default";
    //string[] tires;

    string currentWheel = "default";
    string[] wheels;

    string currentBodyColor = "default";
    string[] colors;

    DrivetrainType drivetrain = DrivetrainType.RWD;
    SuspentionsTypes suspentions = SuspentionsTypes.Default;

    private void OnEnable()
    {
        GetWheels();
    }

    private void GetWheels()
    {
        RearWheel = GameObject.FindGameObjectWithTag("RearWheel");
        FrontWheel = GameObject.FindGameObjectWithTag("FrontWheel");
    }

    //public void UpdateCurrentWheels(Texture2D wheelTexture)
    public void UpdateCurrentWheels(Sprite newSprite)
    {
        GetWheels();
        Vector2 baselineSize = new Vector2(1.1f, 1.1f); // adjust this based on your needs
        UpdateWheelSprite(FrontWheel, newSprite, baselineSize);
        UpdateWheelSprite(RearWheel, newSprite, baselineSize);
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
}
