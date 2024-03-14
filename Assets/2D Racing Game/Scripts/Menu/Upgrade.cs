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

	int id;

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
		id = PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCar);
		CurrentCarImage.sprite = carsIcons[id];
	}

    public void Back ()
    {
		gameObject.SetActive(false);
		SelectMenu.SetActive(true);
    }
	
	public void LoadUpgrade()
	{
		id = PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedCar);
		Engine = PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ());
		Fuel = PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ());
		Suspension = PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ());
		Speed = PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ());

		TorqueTXT.text = "Level: "+ PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()).ToString ()+" / "+enginePrice.Length.ToString();
		SuspensionTXT.text = "Level: "+ PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()).ToString ()+" / "+suspensionPrice.Length.ToString();
		FuelTXT.text = "Level: "+ PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()).ToString ()+" / "+fuelPrice.Length.ToString();
		SpeedTXT.text = "Level: "+ PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()).ToString ()+" / "+speedPrice.Length.ToString();

		CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();


		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()) < enginePrice.Length)
			priceTorqueTXT.text = enginePrice [PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ())].ToString () + " $";
		else
			priceTorqueTXT.text = "Completed";
		
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()) < speedPrice.Length)
			priceSpeedTXT.text = speedPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ())].ToString()+ " $";
		else
			priceSpeedTXT.text = "Completed";
		
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()) < fuelPrice.Length)
			priceFuelTXT.text = fuelPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ())].ToString()+ " $";
		else
			priceFuelTXT.text = "Completed";
		
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()) < suspensionPrice.Length)	
			priceSuspensionTXT.text = suspensionPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ())].ToString()+ " $";
		else
			priceSuspensionTXT.text = "Completed";
		
		
	}
	void Update ()
	{
		#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.H))
			PlayerPrefs.DeleteAll ();
		#endif
	}

	public void EngineUpgrade ()
	{
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()) < enginePrice.Length) {

			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= enginePrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ())]) {
				audioSource.clip = Buy;
				audioSource.Play ();
				PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) - enginePrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ())]);
				PlayerPrefs.SetInt (PlayerPrefsKeys.Engine + id.ToString (), PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()) + 1);
				CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
				TorqueTXT.text = "Level : "+PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()).ToString ()+" / "+enginePrice.Length.ToString();
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ()) < enginePrice.Length)
					priceTorqueTXT.text = enginePrice [PlayerPrefs.GetInt (PlayerPrefsKeys.Engine + id.ToString ())].ToString () + " $";
				else
					priceTorqueTXT.text = "Completed";
			} else {
				Shop.SetActive (true);

				audioSource.clip = Caution;
				audioSource.Play ();
			}

		}
	}

	public void SuspensionUpgrade ()
	{
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()) < suspensionPrice.Length) {

			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= suspensionPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ())]) {
				audioSource.clip = Buy;
				audioSource.Play ();
				PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) - suspensionPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ())]);
				PlayerPrefs.SetInt (PlayerPrefsKeys.Suspension + id.ToString (), PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()) + 1);
				CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
				SuspensionTXT.text = "Level : "+PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()).ToString ()+" / "+suspensionPrice.Length.ToString();
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ()) < speedPrice.Length)
					priceSuspensionTXT.text = suspensionPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Suspension + id.ToString ())].ToString()+ " $";
				else
					priceSuspensionTXT.text = "Completed";
			} else {
				Shop.SetActive (true);
				audioSource.clip = Caution;
				audioSource.Play ();
			}
		}
	}

	public void FuelUpgrade ()
	{
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()) < fuelPrice.Length) {

			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= fuelPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ())]) {
				audioSource.clip = Buy;
				audioSource.Play ();
				PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) - fuelPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ())]);
				PlayerPrefs.SetInt (PlayerPrefsKeys.Fuel + id.ToString (), PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()) + 1);
				CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
				FuelTXT.text = "Level : "+PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()).ToString ()+" / "+fuelPrice.Length.ToString();
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ()) < fuelPrice.Length)
					priceFuelTXT.text = fuelPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Fuel + id.ToString ())].ToString()+ " $";
				else
					priceFuelTXT.text = "Completed";
			} else {
				Shop.SetActive (true);
				audioSource.clip = Caution;
				audioSource.Play ();
			}
		}
	}




	public void SpeedUpgrade ()
	{
		if (PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()) < speedPrice.Length) {

			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= speedPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ())]) {
				audioSource.clip = Buy;
				audioSource.Play ();
				PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) - speedPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ())]);
				PlayerPrefs.SetInt (PlayerPrefsKeys.Speed + id.ToString (), PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()) + 1);
				CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
				SpeedTXT.text = "Level : "+PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()).ToString ()+" / "+speedPrice.Length.ToString();
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ()) < speedPrice.Length)
					priceSpeedTXT.text = speedPrice[PlayerPrefs.GetInt (PlayerPrefsKeys.Speed + id.ToString ())].ToString()+ " $";
				else
					priceSpeedTXT.text = "Completed";
			} else {
				Shop.SetActive (true);
				audioSource.clip = Caution;
				audioSource.Play ();
			}
		}
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
