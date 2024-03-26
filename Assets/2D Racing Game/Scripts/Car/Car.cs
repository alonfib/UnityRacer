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



    //private void UpdateWheels()
    //{
    //    GameObject[] wheels = GameObject.FindGameObjectsWithTag("Wheel");

    //    foreach (GameObject wheel in wheels)
    //    {
    //        // Assuming you can distinguish them based on name
    //        // Adjust the condition based on your actual naming or other distinguishing features
    //        if (wheel.name.Contains("Front"))
    //        {
    //            FrontWheel = wheel;
    //        }
    //        else if (wheel.name.Contains("Rear"))
    //        {
    //            RearWheel = wheel;
    //        }
    //    }

    //    // Check if the wheels were successfully assigned
    //    if (FrontWheel == null)
    //    {
    //        Debug.LogError("Front wheel could not be found.");
    //    }
    //    if (RearWheel == null)
    //    {
    //        Debug.LogError("Rear wheel could not be found.");
    //    }
    //}

}
