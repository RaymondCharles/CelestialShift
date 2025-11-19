using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
    public GameObject LoadingScreen;
    public Slider LoadingBar;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
    }
    public void ChangeToGameScene(int i)
    {
        LoadingScreen.SetActive(true);
        LoadingBar.value = 0;
        StartCoroutine(ChangeSceneAsync(i))   ;

    }

    IEnumerator ChangeSceneAsync(int i) 
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(i);
        while (!asyncLoad.isDone)
        {
            LoadingBar.value = asyncLoad.progress;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f  );
        LoadingScreen.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
