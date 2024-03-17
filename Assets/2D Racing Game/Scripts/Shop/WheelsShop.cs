using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WheelsShop : MonoBehaviour
{
    public CarsManager carsManager;

    public int currentPage = 0;
    public GameObject[] BuyButtons;

    const int BUTTONS_IN_SHOP = 4;

    private void OnEnable()
    {
        UpdateShopPage();
    }

    public void NextPage()
    {
        currentPage++;
        UpdateShopPage();
    }

    public void PrevPage()
    {
        currentPage--;
        UpdateShopPage();
    }

    public void BuyWheel(int selector) //  selector is by the ui count of buttons, 0 - 3
    {
        carsManager.BuyWheel(BUTTONS_IN_SHOP * currentPage + selector);
    }

    void UpdateShopPage()
    {
        for (int i = 0; i < BUTTONS_IN_SHOP; i++)
        {
            int wheelIndex = BUTTONS_IN_SHOP * currentPage + i;
            if (wheelIndex < carsManager.wheelsImages.Length)
            {
                // Assuming each button has a child with an Image component
                RawImage image = BuyButtons[i].GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    image.texture = carsManager.wheelsImages[wheelIndex];
                    BuyButtons[i].SetActive(true);
                }
            }
            else
            {
                // If there's no wheel for this button (e.g., last page with less than 4 items), hide the button
                BuyButtons[i].SetActive(false);
            }
        }
    }
}
