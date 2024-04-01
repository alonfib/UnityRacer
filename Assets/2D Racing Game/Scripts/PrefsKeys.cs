public static class PlayerPrefsKeys
{
    // My New Keys
    public const string New = "New";
    public const string OwnedCarsIds = "OwnedCarsIds";
    public const string OwnedLevelsIds = "OwnedLevelsIds";


    // Existing keys
    public const string Coins = "Coins";
    public const string Level = "Level";
    public const string Car = "Car";
    public const string SelectedCarIndex = "SelectedCarIndex";
    public const string SelectedLevelIndex = "SelectedLevelIndex";

    // Add new keys from MenuTools.cs
    public const string FirstRun = "FirstRun";
    public const string Resolution = "Resolution";
    public const string EngineVolume = "EngineVolume";
    public const string MusicVolume = "MusicVolume";
    public const string CoinAudio = "CoinAudio";
    public const string ShowDistance = "ShowDistance";
    public const string Car0 = "Car0";
    public const string Level0 = "Level0";
    public const string Update = "Update";
    // Add other keys as needed
    public const string Engine = "Engine";
    public const string Fuel = "Fuel";
    public const string Suspension = "Suspension";
    public const string Speed = "Speed";
    public const string AllScoreTemp = "AllScoreTemp";
    public const string Assistance = "Assistance";
}

public static class CarItemsPrefKeys
{
    public const string Car = PlayerPrefsKeys.Car;
    public const string Selected = "Selected";

    // Upgrades Wheels

    public const string Tires = "Tires";
    public const string Wheels = "Wheels";
    public const string Brakes = "Brakes";

    // Customize

    public const string WheelsColor = "WheelsColor";
    public const string CarColor = "CarColor";

    // Body Upgrades

    public const string Drivetrain = "Drivetrain";
    public const string Suspension = "Suspension";
    //public const string Weight = "Weight";

    // Engine Upgrades

    public const string FuelTank = "FuelTank";
    public const string Exhaust = "Exhaust";
    public const string Intake = "Intake";
    public const string Turbo = "Turbo";
    public const string Engine = "Engine";
    //public const string Nitro = "Nitro";
    //public const string TopSpeed = "TopSpeed";
}

