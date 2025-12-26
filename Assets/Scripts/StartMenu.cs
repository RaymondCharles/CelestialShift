using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject PlayerViewScreen;
    public GameObject MainScreen;

    // Change to Player View Screen When Start is Pressed

    public void ChangeScreenToPlayerView()
    {
        PlayerViewScreen.SetActive(true);
    }
    // Change to Main View Screen When Back is Pressed

    public void ChangeScreenToMainScreen()
    {
        PlayerViewScreen.SetActive(false);
    }

    // Change to Game Scene When Play is Pressed
    public void OnClickPlay()
    {
        LoadingManager.Instance.ChangeToGameScene(1);
    }
 



}
