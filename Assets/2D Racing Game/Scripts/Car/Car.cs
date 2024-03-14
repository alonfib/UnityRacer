using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour
{
    public NewCarController controller;
    public DrivetrainType drivetrainType = DrivetrainType.FWD;

    int id;
    Wheel FrontWheel;
    Wheel BackWheel;
    string color = "original";
    //Nitro nitro;

    // Upgrades
    int engineUpgrade = 0;
    int suspentionUpgrade = 0;
    int speedUpgrade = 0;
    int fuelUpgrade = 0;
    //float suspentionHeight = 0;


}
