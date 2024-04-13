using UnityEngine;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{
    public RawImage Image;
    public Text PriceText;
    public Text BottomText;
    public Text TitleText;
    public GameObject PriceGameObject;
    public GameObject LockGameObject;
    public int Price = 0; // Update
    public string CarItem = "";
}
    