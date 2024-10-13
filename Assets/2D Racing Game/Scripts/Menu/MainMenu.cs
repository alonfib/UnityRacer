using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        //ResetData();
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
