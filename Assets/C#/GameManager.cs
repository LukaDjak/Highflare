using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private Camera loaderCamera;
    [SerializeField] private AudioMixer audioMixer;

    [Header("State")]
    public int currentLevel = 1;
    public static bool isGameOver = false;
    public static bool justEnteredGame = true;
    public static Settings settings;

    private string currentSceneName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        LoadSettings();
        ApplyAudioSettings();
    }

    private void Start() => ApplyAudioSettings();

    public void LoadScene(string loadSceneName, string unloadSceneName = null)
    {
        string sceneToUnload = unloadSceneName ?? currentSceneName;

        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            loaderCamera.gameObject.SetActive(true);
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        loadOp.completed += _ =>
        {
            loaderCamera.gameObject.SetActive(false);
            currentSceneName = loadSceneName;
        };
    }

    private void LoadSettings()
    {
        settings ??= new Settings
        {
            sensX = PlayerPrefs.GetFloat("SensX", 1f),
            sensY = PlayerPrefs.GetFloat("SensY", 1f),
            audioVolume = PlayerPrefs.GetFloat("Audio", 1f),
            musicVolume = PlayerPrefs.GetFloat("Music", 1f)
        };
        currentLevel = PlayerPrefs.GetInt("Level", 1);
    }

    private void ApplyAudioSettings()
    {
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(settings.audioVolume, 0.001f, 1f)) * 20f);
        audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Clamp(settings.musicVolume, 0.001f, 1f)) * 20f);
    }

    private void OnApplicationQuit() => SaveSettings();

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetFloat("SensX", settings.sensX);
        PlayerPrefs.SetFloat("SensY", settings.sensY);
        PlayerPrefs.SetFloat("Audio", settings.audioVolume);
        PlayerPrefs.SetFloat("Music", settings.musicVolume);
        PlayerPrefs.Save();
    }
}

public class Settings
{
    public float sensX;
    public float sensY;
    public float audioVolume;
    public float musicVolume;
}