using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool loadGame = false;
    public bool inGame = false;
    public AudioClip menuMusic;
    public AudioClip inGameMusic;
    public AudioSource currentMusic;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    private float savedTime = 0f;
    public float loopGap = 20f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentMusic.clip = menuMusic;
        currentMusic.volume = volume;
        currentMusic.playOnAwake = false;
    }

    private void Start()
    {
        volume = PlayerPrefs.HasKey("MusicVol") ? PlayerPrefs.GetFloat("MusicVol") : 0.6f;

        if (currentMusic.clip)
        {
            currentMusic.volume = volume;
            currentMusic.Play();
        }
    }
    
    private void Update()
    {
        if (inGame == true && currentMusic.clip != inGameMusic)
        {
            currentMusic.Stop();
            currentMusic.clip = inGameMusic;
            currentMusic.time = savedTime;
            currentMusic.Play();
        }
        if (inGame == false && currentMusic.clip != menuMusic)
        {
            savedTime = currentMusic.time;
            currentMusic.Stop();
            currentMusic.clip = menuMusic;
            currentMusic.Play();
        }
        if (!currentMusic.isPlaying)
        {
            Invoke("PlayMusic", loopGap);
        }
    }


    public void PlayMusic()
    {
        if (!currentMusic.isPlaying)
        {
            currentMusic.Play();
        }
    }
/*

    public void FadeTo(float targetVolume, float seconds)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(targetVolume, seconds));
    }

    private System.Collections.IEnumerator FadeRoutine(float target, float seconds)
    {
        float start = currentMusic.volume;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            currentMusic.volume = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }
        currentMusic.volume = target;
    }


*/



    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadGame && scene.name == "SampleScene")
        {
            StartCoroutine(WaitForPlayerAndLoad());
        }
    }

    private IEnumerator WaitForPlayerAndLoad()
    {
        // Wait until the FirstPersonController exists
        while (FirstPersonController.Instance == null)
            yield return null;

        // Let the player load itself
        FirstPersonController.Instance.LoadPlayer();
        inGame = true;
    }

    public void NewGame()
    {
        loadGame = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGame()
    {
        savedTime = 0f;
        if (!SaveSystem.SaveExists())
        {
            Debug.LogWarning("No save found!");
            return;
        }

        loadGame = true;
        SceneManager.LoadScene("SampleScene");
    }


}
