using System;

public static class CarUtils // A class to hold utility methods related to cars
{
    // Encapsulates your CarIdFormatter method inside a class
    public static string CarIdFormatter(string carId, string carPrefKey)
    {
        return carId + carPrefKey;
    }
}

    // Shop Items

public static class DrivetrainType
{
    public const string RWD = "RWD";
    public const string FWD = "FWD";    
    public const string AWD = "AWD";    
}

public static class SuspentionsTypes
{
    public const string Default = "Default";
    public const string Sport = "Sport";
    public const string Rally = "Rally";
    public const string High = "High";
}

public static class BrakesTypes
{
    public const string Default = "Default";
    public const string Sport = "Sport";
    public const string StreetRacing = "StreetRacing";
    public const string Racing = "Racing";
}

public static class TiresTypes
{
    public const string Default = "Default";
    public const string Sport = "Sport";
    public const string StreetRacing = "StreetRacing";
    public const string Racing = "Racing";
}

