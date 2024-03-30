using System;

public static class CarUtils // A class to hold utility methods related to cars
{
    // Encapsulates your CarIdFormatter method inside a class
    public static string CarIdFormatter(string carId, string carPrefKey)
    {
        return carId + carPrefKey;
    }
}

public enum DrivetrainType
{
    FWD, // Front Wheel Drive
    RWD, // Rear Wheel Drive
    AWD  // All Wheel Drive
}

public enum SuspentionsTypes
{
    Default,
    Medium,
    High,
    Low
}

