using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject PlayerViewScreen;
    public GameObject MainScreen;
    public GameObject PauseScreen;

    public void ChangeScreenToPlayerView()
    {
        PlayerViewScreen.SetActive(true);
    }
    public void ChangeScreenToMainScreen()
    {
        PlayerViewScreen.SetActive(false);
    }
    public void OnClickPlay()
    {
        LoadingManager.Instance.ChangeToGameScene(1);
    }
 



}
