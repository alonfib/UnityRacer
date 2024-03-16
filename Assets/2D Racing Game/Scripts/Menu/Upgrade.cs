using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Upgrade : MonoBehaviour
{
	[HideInInspector]public int Engine, Fuel, Suspension, Speed;

	[Header("Upgrades price")]
	public int[] enginePrice;
	public int[] fuelPrice;
	public int[] suspensionPrice;
	public int[] speedPrice;

	int selectedCarId;

	[Header("Informatin Texts")]
	public Text CoinsTXT;
	public Text TorqueTXT, SuspensionTXT, FuelTXT, SpeedTXT;
	public Text priceTorqueTXT, priceSuspensionTXT, priceFuelTXT, priceSpeedTXT;

	[Header("Show Window")]
	public GameObject Shop;
	public GameObject Loading;
	public GameObject SelectMenu;
	public Image CurrentCarImage;
	public Sprite[] carsIcons;

	[Header("Sound Clips")]
	public AudioClip Buy, Caution;
	public AudioSource audioSource;

	[Header("Control Assistance CheakBox")]
	public Toggle ControllAsist;

	void Awake ()
	{
		LoadUpgrade ();
	}

    private void OnEnable()
    {
		selectedCarId = PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCar);
		CurrentCarImage.sprite = carsIcons[selectedCarId];
	}

    public void Back ()
    {
		gameObject.SetActive(false);
		SelectMenu.SetActive(true);
    }
	
	public void LoadUpgrade()
	{
		selectedCarId = PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedCar);
		Engine = PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + selectedCarId.ToString ());
		Fuel = PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + selectedCarId.ToString ());
		Suspension = PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + selectedCarId.ToString ());
		Speed = PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + selectedCarId.ToString ());

		UpdateUI();
	}

	private void UpdateUI()
	{
		TorqueTXT.text = FormatLevelText(Engine, enginePrice.Length);
		SuspensionTXT.text = FormatLevelText(Suspension, suspensionPrice.Length);
		FuelTXT.text = FormatLevelText(Fuel, fuelPrice.Length);
		SpeedTXT.text = FormatLevelText(Speed, speedPrice.Length);

		CoinsTXT.text = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins).ToString();

		UpdatePriceText(priceTorqueTXT, Engine, enginePrice);
		UpdatePriceText(priceSpeedTXT, Speed, speedPrice);
		UpdatePriceText(priceFuelTXT, Fuel, fuelPrice);
		UpdatePriceText(priceSuspensionTXT, Suspension, suspensionPrice);
	}

	private void UpdatePriceText(Text priceText, int level, int[] prices)
	{
		priceText.text = level < prices.Length ? prices[level] + " $" : "Completed";
	}

	private string FormatLevelText(int level, int maxLevel)
	{
		return "Level: " + level.ToString() + " / " + maxLevel.ToString();
	}

	void Update ()
	{
		#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.H))
			PlayerPrefs.DeleteAll ();
		#endif
	}

	private int GetPlayerPrefInt(string key)
	{
		return PlayerPrefs.GetInt(key);
	}

	private void SetPlayerPrefInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
		PlayerPrefs.Save();
	}

	private void PlaySound(AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.Play();
	}

	private void UpgradeFeature(string featureKey, int[] prices, System.Action incrementFeatureLevel)
	{
		int featureLevel = GetPlayerPrefInt(featureKey + selectedCarId.ToString());
		if (featureLevel < prices.Length)
		{
			int price = prices[featureLevel];
			int coins = GetPlayerPrefInt(PlayerPrefsKeys.Coins);
			if (coins >= price)
			{
				SetPlayerPrefInt(PlayerPrefsKeys.Coins, coins - price);
				incrementFeatureLevel();
				SetPlayerPrefInt(featureKey + selectedCarId.ToString(), featureLevel + 1);
				PlaySound(Buy);
				UpdateUI();
			}
			else
			{
				Shop.SetActive(true);
				PlaySound(Caution);
			}
		}
	}

	public void EngineUpgrade()
	{
		UpgradeFeature(PlayerPrefsKeys.Engine, enginePrice, () => Engine++);
	}

	public void SuspensionUpgrade()
	{
		UpgradeFeature(PlayerPrefsKeys.Suspension, suspensionPrice, () => Suspension++);
	}

	public void FuelUpgrade()
	{
		UpgradeFeature(PlayerPrefsKeys.Fuel, fuelPrice, () => Fuel++);
	}

	public void SpeedUpgrade()
	{
		UpgradeFeature(PlayerPrefsKeys.Speed, speedPrice, () => Speed++);
	}

	public void StartGame ()
	{
		
		Loading.SetActive (true);
		PlayerPrefs.SetInt (PlayerPrefsKeys.AllScoreTemp, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins));
		SceneManager.LoadSceneAsync ("Level"+PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedLevel).ToString());
		//sceneLoading.ActivateNextScene();

		gameObject.SetActive (false);

	}


	public void SetControll ()
	{
		StartCoroutine (ControllAsistanceSave ());
	}

	IEnumerator ControllAsistanceSave ()
	{
		yield return new WaitForEndOfFrame ();

		if (ControllAsist.isOn)
			PlayerPrefs.SetInt (PlayerPrefsKeys.Assistance, 3);   // 3=>true - 0=>false    
		else
			PlayerPrefs.SetInt (PlayerPrefsKeys.Assistance, 0);   // 3=>true - 0=>false    
	}
}
