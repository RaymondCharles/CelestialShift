using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public GameObject PlayerViewScreen;
    public GameObject MainScreen;
    public GameObject PauseScreen;

    public void ChangeScreenToPlayerView()
    {
        Debug.Log("Start Button pressed");
        PlayerViewScreen.SetActive(true);
    }
    public void ChangeScreenToMainScreen()
    {
        Debug.Log("Back Button pressed");
        PlayerViewScreen.SetActive(false);
    }
    public void ChangeScreenToGameScreen()
    {
        PlayerViewScreen.SetActive(false);
        MainScreen.SetActive(false);
    }
    public void ChangeScreenToGamePlayScreen()
    {
        PlayerViewScreen.SetActive(false);
        MainScreen.SetActive(false);
        PauseScreen.SetActive(false);
    }

}
