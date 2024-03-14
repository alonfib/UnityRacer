using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class UnityAdsIntegration : MonoBehaviour
{

	[SerializeField] private string androidGameId = "18658"; //ID for testing
	[SerializeField] private string iOSGameId = "18658"; //ID for testing
	//[SerializeField] private string androidGameId = "5570375"; //REAL ID 
	//[SerializeField] private string iOSGameId = "5570374"; //REAL ID
	public string zoneId = "rewardedVideo";

	[Space(3)]
	[SerializeField] bool enableTestMode;

	void Start ()
	{
		int numb = PlayerPrefs.GetInt("Number");

		numb++;

		if(numb == 4)
			numb = 0;
		
		PlayerPrefs.SetInt("Number",numb);

		string gameId = null;

		#if UNITY_ANDROID
		gameId = androidGameId;
		#else
		gameId = iOSGameId;
		#endif

		if(!Advertisement.isInitialized) {
			Advertisement.Initialize(gameId, enableTestMode);

		} 

		if(numb == 3)
			Show();
	}




	public void Show()
	{
		
		ShowOptions options = new ShowOptions();

		//options.resultCallback = HandleShowResult;
			
		Advertisement.Show(zoneId, options);

	}



	private void HandleShowResult (ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			break;
		case ShowResult.Skipped:
			break;
		case ShowResult.Failed:
			break;
		}
	}


	public void LoadLevelName(string name)
	{

		UnityEngine.SceneManagement.SceneManager.LoadScene (name);
	}
}