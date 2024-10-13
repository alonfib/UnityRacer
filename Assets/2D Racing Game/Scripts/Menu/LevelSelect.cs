using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [Header("Icons")]
    public Sprite[] levelIcons; // Assuming these are the icons for each level
    public Image currentItemImage;
    public Image prevItemImage;
    public Image nextItemImage;
    public Image topRightImage;

    [Header("Current Item Animation")]
    public Animator currentAnimator;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip okClip;
    public AudioClip errorClip;

    [Header("Texts")]
    public Text currentItem;
    public Text coinsTXT;

    [Header("Windows")]
    public GameObject shopOffer;
    public GameObject lockIcon;
    public GameObject nextMenu;
    public GameObject mainMenu;

    // List of the items price
    public int[] levelsPrices;

    int selectedLevelIndex;
    bool canAnim;
    bool animaState;


    void OnEnable()
    {

        coinsTXT.text = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins).ToString();

        // Read last selected level ID
        selectedLevelIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedLevelIndex);

        UpdateImages();
    }

    private void UpdateImages()
    {
        currentItemImage.sprite = levelIcons[selectedLevelIndex];
        topRightImage.sprite = levelIcons[selectedLevelIndex];

        nextItemImage.sprite = selectedLevelIndex < levelIcons.Length - 1 ? levelIcons[selectedLevelIndex + 1] : null;
        nextItemImage.color = selectedLevelIndex < levelIcons.Length - 1 ? Color.white : Color.clear;

        prevItemImage.sprite = selectedLevelIndex > 0 ? levelIcons[selectedLevelIndex - 1] : null;
        prevItemImage.color = selectedLevelIndex > 0 ? Color.white : Color.clear;

        lockIcon.SetActive(PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) != PlayerPrefsKeys.OwnedValue);
        currentItem.text = levelsPrices[selectedLevelIndex].ToString();
    }

    public void NextItem()
    {
        if (selectedLevelIndex < levelIcons.Length - 1)
        {
            selectedLevelIndex++;
            PlayAnim();
            UpdateImages();
            PlayClip(okClip);
        }
    }

    public void PrevItem()
    {
        if (selectedLevelIndex > 0)
        {
            selectedLevelIndex--;
            PlayAnim();

            UpdateImages();
            PlayClip(okClip);
        }
    }

    void PlayAnim()
    {
        animaState = !animaState;
        currentAnimator.CrossFade(animaState ? "Next" : "Prev", .003f);
    }

    private void PlayClip(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void BuyLevel()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) != PlayerPrefsKeys.OwnedValue)
        {
            if (PlayerPrefs.GetInt(PlayerPrefsKeys.Coins) >= levelsPrices[selectedLevelIndex])
            {
                int newCoinsCount = PlayerPrefs.GetInt(PlayerPrefsKeys.Coins) - levelsPrices[selectedLevelIndex];
                PlayerPrefs.SetInt(PlayerPrefsKeys.Coins, newCoinsCount);
                PlayerPrefs.SetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString(), PlayerPrefsKeys.OwnedValue);
                lockIcon.SetActive(false);
                coinsTXT.text = newCoinsCount.ToString();
                PlayClip(okClip);
            }
            else
            {
                shopOffer.SetActive(true);
                PlayClip(errorClip);
            }
        }
    }

    public void SelectCurrent()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.Level + selectedLevelIndex.ToString()) == PlayerPrefsKeys.OwnedValue)
        {
            // Logic when level is already owned
            nextMenu.SetActive(true);
            PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedLevelIndex, selectedLevelIndex);
            PlayerPrefs.Save();
        }
        else
        {
            // Attempt to buy if not owned
            BuyLevel();
        }
    }

    public void Back()
    {
        gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }
}
