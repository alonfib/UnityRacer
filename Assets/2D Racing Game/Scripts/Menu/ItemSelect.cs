using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum ItemType
{
	Level,
	Car
}
public class ItemSelect : MonoBehaviour
{

	// Item type , Level or Car
	public ItemType itemType;
	public CarsManager carsManager;
	// Current,Prev and next button images + Items icon
	[Header("Icons")]
	public Sprite[] levelIcons;
	public Image currentItemImage;
	public Image prevItemImage;
	public Image nextItemImage;
	public Image topRightImage;


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
	int selectedCarIndex;
	bool canAnim;
	bool animaState;

	// List of the items price
	public int[] itemsPrice;
	Sprite[] carsIcons;

	void Start ()
	{
		carsIcons = carsManager.carsIcons;
		itemType = ItemType.Level;

		// Display total coins on start
		coinsTXT.text = PlayerPrefs.GetInt (PlayerPrefsKeys.Coins).ToString ();

		// Read last selected item ID
		selectedCarIndex = PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedCarIndex);
		selectedLevelIndex = PlayerPrefs.GetInt (PlayerPrefsKeys.SelectedLevelIndex);

		UpdateImages();

		// Internal usage
		canAnim = true;
		// Check current item is unlocked?
		bool isLocked = itemType == ItemType.Car ? !carsManager.IsCarOwned(selectedCarIndex) : PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) != 3;
		if(isLocked)
        {
			lockIcon.SetActive(true);
        }
		else
        {
			lockIcon.SetActive(false);
		}

 
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
		Sprite[] icons = itemType == ItemType.Car ? carsManager.carsIcons : levelIcons;
		int selectedId = itemType == ItemType.Car ? selectedCarIndex : selectedLevelIndex;
		// Update current and top right image
		currentItemImage.sprite = icons[selectedId];
		topRightImage.sprite = icons[selectedId];

		if(itemType  == ItemType.Car)
        {
			carsManager.currentCarIndex = selectedCarIndex;
		}

		// Update next image
		if (selectedId < icons.Length - 1)
		{
			nextItemImage.sprite = icons[selectedId + 1];
			nextItemImage.color = new Color(1f, 1f, 1f, 1f); // Make sure the next item is visible
		}
		else
		{
			nextItemImage.sprite = null;	
			nextItemImage.color = new Color(0, 0, 0, 0); // Hide the next item if it doesn't exist
		}

		// Update previous image
		if (selectedId > 0)
		{
			prevItemImage.sprite = icons[selectedId - 1];
			prevItemImage.color = new Color(1f, 1f, 1f, 1f); // Make sure the previous item is visible
		}
		else
		{
			prevItemImage.sprite = null;
			prevItemImage.color = new Color(0, 0, 0, 0); // Hide the previous item if it doesn't exist
		}

		bool isLocked = itemType == ItemType.Car ? !carsManager.IsCarOwned(selectedCarIndex) : PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedId.ToString()) != 3;
		if (isLocked)
			lockIcon.SetActive(true);
		else
			lockIcon.SetActive(false);
	}

	// public function used in ui button to select next car
	public void NextCar ()
	{
		Sprite[] icons = itemType == ItemType.Car ? carsIcons : levelIcons;
		int selectedId = itemType == ItemType.Car ? selectedCarIndex : selectedLevelIndex;

		if (selectedId < icons.Length - 1) {
			if (itemType == ItemType.Car)
				selectedCarIndex++;
			else if (itemType == ItemType.Level)
				selectedLevelIndex++;
			if (canAnim)
				PlayAnim();

			PlayerPrefs.SetInt(itemType == ItemType.Car ? PlayerPrefsKeys.SelectedCarIndex : PlayerPrefsKeys.SelectedLevelIndex, selectedId);
			currentItem.text = itemType == ItemType.Car ? carsManager.carPrices[selectedCarIndex].ToString() : itemsPrice[selectedLevelIndex].ToString();
			PlayClip(okClip);
			UpdateImages();
		}
	}

	// public function used in ui button to select prev car
	public void PrevCar ()
	{
		int selectedId = itemType == ItemType.Car ? selectedCarIndex : selectedLevelIndex;

		if (selectedId > 0) {
			if (itemType == ItemType.Car)	
				selectedCarIndex--;
			else if (itemType == ItemType.Level)
				selectedLevelIndex--;
			if (canAnim)
				PlayAnim ();

			PlayerPrefs.SetInt(itemType == ItemType.Car ? PlayerPrefsKeys.SelectedCarIndex : PlayerPrefsKeys.SelectedLevelIndex, selectedId);
			currentItem.text = itemType == ItemType.Car ? carsManager.carPrices[selectedCarIndex].ToString() : itemsPrice[selectedLevelIndex].ToString();
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



	public void Next()
	{
		currentItem.text = itemType == ItemType.Car ? carsManager.carPrices[selectedCarIndex].ToString() : itemsPrice[selectedLevelIndex].ToString();

		if (itemType == ItemType.Level)
		{
			bool isGameOwned = PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) == 3;
			if (isGameOwned)
			{
				itemType = ItemType.Car;
				UpdateImages();
				PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedLevelIndex, selectedLevelIndex);
			}
			else
			{
				PlayClip(errorClip);
			}
		} else if(itemType == ItemType.Car)
        {
			Debug.Log("Next Func" + selectedCarIndex); // Debug statement
			PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCarIndex, selectedCarIndex);
			gameObject.SetActive(false);
			nextMen.SetActive(true);
		}

	}

	public void Back()
	{
		currentItem.text = itemType == ItemType.Car ? carsManager.carPrices[selectedCarIndex].ToString() : itemsPrice[selectedLevelIndex].ToString();

		if (itemType == ItemType.Car)
		{
			itemType = ItemType.Level;
			UpdateImages();
		}
		else if (itemType == ItemType.Level)
		{
			gameObject.SetActive(false);
			mainMenu.SetActive(true);
		}
	}

	// Select current item and go to the next menu
	public void SelectCurrent ()
	{
		if (itemType == ItemType.Level) {
			bool isGameOwned = PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) == 3;
			if (isGameOwned) {
				itemType = ItemType.Car;
				UpdateImages();
				PlayerPrefs.SetInt (PlayerPrefsKeys.SelectedLevelIndex, selectedLevelIndex);
			} else {
				PlayClip(errorClip);
			}
		}
        if (itemType == ItemType.Car)
        {
            if (carsManager.IsCarOwned(selectedCarIndex))
            {
                gameObject.SetActive(false);
                nextMen.SetActive(true);
                PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCarIndex, selectedCarIndex);
            }
            else
            {
                PlayClip(errorClip);
            }
        }
    }

	// Public function used in current selected button (ui button ) 
	public void Buy ()
	{

		if (itemType == ItemType.Level) {
			if (PlayerPrefs.GetInt (PlayerPrefsKeys.Level + selectedLevelIndex.ToString ()) != 3) {
				if (PlayerPrefs.GetInt (PlayerPrefsKeys.Coins) >= itemsPrice [selectedLevelIndex]) {
					int newCoinsCount = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins) - itemsPrice[selectedLevelIndex];
					PlayerPrefs.SetInt (PlayerPrefsKeys.Coins, newCoinsCount);
					PlayerPrefs.SetInt (PlayerPrefsKeys.Level + selectedLevelIndex.ToString (), 3);
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
