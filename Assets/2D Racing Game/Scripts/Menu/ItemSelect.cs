using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum ItemType
{
	Level,
}
public class ItemSelect : MonoBehaviour
{

	// Item type , Level or Car
	// Current,Prev and next button images + Items icon
	[Header("Icons")]
	public Sprite[] levelIcons;
	public Image currentItemImage;
	public Image prevItemImage;
	public Image nextItemImage;
	public Image topRightImage;

	public GameObject SelectButton;
	public GameObject BuyButton;
	public GameObject LockedScreen;
	public Text GamePriceText;


	// We used animator to animated current item when selected
	[Header("Current Item Animation")]
	public Animator currentAnimator;

	// play error sound clip when player coins is not enough, OK sound clip when player has enough money and buy item then
	[Header("Sounds")]
	public AudioSource audioSource;
	public AudioClip okClip ;
	public AudioClip errorClip;

	// Display cuurent item an total coins
	[Header("Texts")]
	public Text currentItem;
	public Text coinsTXT;

	// Activate these windows when we need them
	[Header("Windows")]
	public GameObject shopOffer;
	public GameObject lockIcon;
	public GameObject nextMen;
	public GameObject mainMenu;

	// Internal usage
	int selectedLevelIndex;
	bool canAnim;
	bool animaState;

	ItemType itemType = ItemType.Level;
	bool isLocked;

	// List of the items price
	public int[] itemsPrice;

	void Start ()
	{

		// Display total coins on start
		coinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
		selectedLevelIndex = PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedLevelIndex);


		// Internal usage
		canAnim = true;
		// Check current item is unlocked?


		UpdateImages();

		// Update item prices
		currentItem.text = itemsPrice [selectedLevelIndex].ToString ();
	}

	private void PlayClip(AudioClip audioClip)
    {
		audioSource.clip = audioClip;
		audioSource.Play();
	}


	private void UpdateImages()
	{
		isLocked = CheckIfLocked();
		if (isLocked)
		{
			GamePriceText.text = itemsPrice[selectedLevelIndex].ToString();

			lockIcon.SetActive(true);
			BuyButton.SetActive(true);
			LockedScreen.SetActive(true);
			SelectButton.SetActive(false);
		}
		else
		{
			lockIcon.SetActive(false);
			LockedScreen.SetActive(false);
			BuyButton.SetActive(false);
			SelectButton.SetActive(true);
		}

		Sprite[] icons = levelIcons;
		// Update current and top right image
		currentItemImage.sprite = icons[selectedLevelIndex];
		topRightImage.sprite = icons[selectedLevelIndex];

	

		// Update next image
		if (selectedLevelIndex < icons.Length - 1)
		{
			nextItemImage.sprite = icons[selectedLevelIndex + 1];
			nextItemImage.color = new Color(1f, 1f, 1f, 1f); // Make sure the next item is visible
		}
		else
		{
			nextItemImage.sprite = null;	
			nextItemImage.color = new Color(0, 0, 0, 0); // Hide the next item if it doesn't exist
		}

		// Update previous image
		if (selectedLevelIndex > 0)
		{
			prevItemImage.sprite = icons[selectedLevelIndex - 1];
			prevItemImage.color = new Color(1f, 1f, 1f, 1f); // Make sure the previous item is visible
		}
		else
		{
			prevItemImage.sprite = null;
			prevItemImage.color = new Color(0, 0, 0, 0); // Hide the previous item if it doesn't exist
		}
	}

	private bool CheckIfLocked()
    {
		isLocked = PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) != PlayerPrefsKeys.OwnedValue;
		return isLocked;
	}

	// public function used in ui button to select next car
	public void Next ()
	{
		Sprite[] icons =  levelIcons;

		if (selectedLevelIndex < icons.Length - 1) {
			selectedLevelIndex++;
			if (canAnim)
				PlayAnim();

			currentItem.text =  itemsPrice[selectedLevelIndex].ToString();
			PlayClip(okClip);
			UpdateImages();
		}
	}

	// public function used in ui button to select prev car
	public void Prev ()
	{
		if (selectedLevelIndex > 0) {
			selectedLevelIndex--;
			if (canAnim)
				PlayAnim ();

			currentItem.text = itemsPrice[selectedLevelIndex].ToString();
			PlayClip(okClip);
			UpdateImages();
		}
	}


	// Play animation when player select next or prev item
	void PlayAnim ()
	{
		animaState = !animaState;
		if (animaState)
			currentAnimator.CrossFade ("Next", .003f);
		else
			currentAnimator.CrossFade ("Prev", .003f);

	}


	public void Back()
	{
		currentItem.text = itemsPrice[selectedLevelIndex].ToString();

        if (itemType == ItemType.Level)
		{
			gameObject.SetActive(false);
			mainMenu.SetActive(true);
		}
	}

	// Select current item and go to the next menu
	public void SelectCurrent ()
	{
		if (itemType == ItemType.Level) {
			bool isLevelOwned = PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) == PlayerPrefsKeys.OwnedValue;
			if (isLevelOwned) {
				UpdateImages();
				PlayerPrefs.SetInt (PlayerPrefsKeys.SelectedLevelIndex, selectedLevelIndex);
			} else {
				PlayClip(errorClip);
			}
		}
    
    }

	// Public function used in current selected button (ui button ) 
	public void Buy ()
	{

		if (itemType == ItemType.Level) {
			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Level + selectedLevelIndex.ToString ()) != PlayerPrefsKeys.OwnedValue) {
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= itemsPrice [selectedLevelIndex]) {
					int newCoinsCount = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins) - itemsPrice[selectedLevelIndex];
					PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, newCoinsCount);
					PlayerPrefs.SetInt (PlayerPrefsKeys.Level + selectedLevelIndex.ToString (), PlayerPrefsKeys.OwnedValue);
					lockIcon.SetActive (false);
					coinsTXT.text = newCoinsCount.ToString ();
				} else
					shopOffer.SetActive (true);
			}
		}

			//if (itemType == ItemType.Car) {
			//	carsManager.BuyCar(selectedCarIndex, () =>
			//	{
			//	lockIcon.SetActive (false);
			//	coinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();
			//	});
			//}
	}
}
