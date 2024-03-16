using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuTools : MonoBehaviour {



	// Player starting game in first time score
	public int startScore;

	public Text CoinsTXT;

	[Header("Menu Resolution")]
	public int ResolutionX = 1280;
	public int ResolutionY = 720;

	public GameObject manuMusic;

	void Start () {
	
		if(GameObject.Find ("LevelMusic(Clone)"))
			Destroy (GameObject.Find ("LevelMusic(Clone)"));

		if(!GameObject.Find("MenuMusic(Clone)"))
			Instantiate (manuMusic, Vector3.zero, Quaternion.identity);
		
		if (PlayerPrefs.GetString (PlayerPrefsKeys.FirstRun) != "True") {

			PlayerPrefs.SetString (PlayerPrefsKeys.FirstRun, "True");
			PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) + startScore);

			PlayerPrefs.SetInt (PlayerPrefsKeys.Resolution, 2);// 3 => true | 0 => false

			PlayerPrefs.SetFloat (PlayerPrefsKeys.EngineVolume, 0.74f);
			PlayerPrefs.SetFloat (PlayerPrefsKeys.MusicVolume, 1f);
			PlayerPrefs.SetInt (PlayerPrefsKeys.ShowDistance, 3);
			PlayerPrefs.SetInt (PlayerPrefsKeys.CoinAudio, 3); 

			PlayerPrefs.SetInt (PlayerPrefsKeys.Car0, 3);// 3 => true | 0 => false
			PlayerPrefs.SetInt (PlayerPrefsKeys.Level0, 3);// 3 => true | 0 => false

		}

		if (PlayerPrefs.GetString (PlayerPrefsKeys.Update) != "True") {
			PlayerPrefs.SetString (PlayerPrefsKeys.FirstRun, "True");
			PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) + startScore);
		}


		if(CoinsTXT)
        {
			CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
        }
	}
	

	void Update () {
		if (Input.GetKeyDown (KeyCode.H)) {
			PlayerPrefs.DeleteAll ();
			CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
			#if UNITY_EDITOR
			Debug.Log("PlayerPrefs.DeleteAll");
			#endif

		}
		if (Input.GetKeyDown (KeyCode.V)) {
			PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) + startScore);
			CoinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
			#if UNITY_EDITOR
			Debug.Log(PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString()); 
			#endif

		}
	}

	public void OpenGooglePlay(string packageName){
		if(Application.platform == RuntimePlatform.Android){
			Application.OpenURL("market://details?id="+packageName);
		}else{
			Application.OpenURL("https://play.google.com/store/apps/details?id="+packageName);
		}
	}

	public void RateAPP(string packageName)
	{
		OpenGooglePlay(packageName);
	}

	public void SetTrue(GameObject target)
	{
		target.SetActive (true);
	}
	public void SetFalse(GameObject target)
	{
		target.SetActive (false);
	}
	public void ToggleObject(GameObject target)
	{
		target.SetActive (!target.activeSelf);
	}
	public void LoadLevel(string name)
	{
		SceneManager.LoadScene (name);
	}
	public void OpenURL(string url)
	{
		Application.OpenURL (url);
	}
	public void LoadLevelAsync(string name)
	{
		SceneManager.LoadSceneAsync (name);
	}
	public void Exit()
	{
		Application.Quit ();
	}

}
	