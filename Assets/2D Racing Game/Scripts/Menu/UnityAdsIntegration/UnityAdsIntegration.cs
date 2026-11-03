using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Unity Ads 4.x flow: Initialize -> Load(placement) -> Show(placement).
// Show only works after the placement has finished loading, so Show() queues the
// request and plays the ad as soon as it is loaded.
// NOTE: the Game IDs actually used are the values serialized on the
// UnityAdsIntegration object in Scenes/UnityAds.unity, not the defaults below.
public class UnityAdsIntegration : MonoBehaviour,
	IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
	[SerializeField] private string androidGameId = "18658"; //ID for testing
	[SerializeField] private string iOSGameId = "18658"; //ID for testing
	//[SerializeField] private string androidGameId = "5570375"; //REAL ID
	//[SerializeField] private string iOSGameId = "5570374"; //REAL ID
	public string zoneId = "rewardedVideo";

	[Space(3)]
	[SerializeField] bool enableTestMode;

	[Header("Optional UI")]
	public Text StatusText;
	public Text AwardText;
	[Tooltip("Coins given when a rewarded ad is watched to the end. 0 = no reward.")]
	public int AwardedCoins = 0;

	private bool isLoaded;
	private bool showWhenLoaded;

	void Start ()
	{
		int numb = PlayerPrefs.GetInt("Number");

		numb++;

		if(numb == 4)
			numb = 0;

		PlayerPrefs.SetInt("Number",numb);

		// Every 4th visit to this scene plays an ad automatically.
		showWhenLoaded = numb == 3;

		string gameId;

		#if UNITY_ANDROID
		gameId = androidGameId;
		#else
		gameId = iOSGameId;
		#endif

		if (!Advertisement.isSupported || string.IsNullOrEmpty(gameId))
		{
			SetStatus("Ads are not available");
			return;
		}

		if (Advertisement.isInitialized)
			LoadAd();
		else
		{
			SetStatus("Loading ad...");
			Advertisement.Initialize(gameId, enableTestMode, this);
		}
	}

	public void Show()
	{
		if (isLoaded)
		{
			isLoaded = false;
			Advertisement.Show(zoneId, this);
		}
		else
		{
			showWhenLoaded = true;
			SetStatus("Loading ad...");
			if (Advertisement.isInitialized)
				LoadAd();
		}
	}

	public void LoadLevelName(string name)
	{
		SceneManager.LoadScene (name);
	}

	private void LoadAd()
	{
		Advertisement.Load(zoneId, this);
	}

	private void SetStatus(string message)
	{
		if (StatusText != null)
			StatusText.text = message;
	}

	// IUnityAdsInitializationListener
	public void OnInitializationComplete()
	{
		LoadAd();
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
		Debug.LogWarning($"Unity Ads initialization failed: {error} - {message}");
		SetStatus("Ads are not available");
	}

	// IUnityAdsLoadListener
	public void OnUnityAdsAdLoaded(string placementId)
	{
		if (placementId != zoneId)
			return;

		isLoaded = true;
		SetStatus("");

		if (showWhenLoaded)
		{
			showWhenLoaded = false;
			Show();
		}
	}

	public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
	{
		Debug.LogWarning($"Unity Ads failed to load '{placementId}': {error} - {message}");
		SetStatus("No ad available right now");
	}

	// IUnityAdsShowListener
	public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
	{
		Debug.LogWarning($"Unity Ads failed to show '{placementId}': {error} - {message}");
		SetStatus("No ad available right now");
		LoadAd();
	}

	public void OnUnityAdsShowStart(string placementId) { }

	public void OnUnityAdsShowClick(string placementId) { }

	public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
	{
		if (placementId == zoneId && showCompletionState == UnityAdsShowCompletionState.COMPLETED && AwardedCoins > 0)
		{
			PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, PlayerPrefs.GetInt(PlayerPrefsKeys.Coins) + AwardedCoins);
			PlayerPrefs.Save();
			if (AwardText != null)
				AwardText.text = "+" + AwardedCoins;
		}

		// Get the next ad ready.
		LoadAd();
	}
}
