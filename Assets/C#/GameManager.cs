using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Camera loaderCamera;

    [HideInInspector] public int currentLevel = 1;
    public static bool isGameOver = false;
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
        ApplySettings();

        // LoadScene("MainMenu");
    }

    public void LoadScene(string loadSceneName, string unloadSceneName = null)
    {
        string sceneToUnload = unloadSceneName ?? currentSceneName;

        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            loaderCamera.gameObject.SetActive(true);
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ =>
        {
            loaderCamera.gameObject.SetActive(false);
            currentSceneName = loadSceneName;
        };
    }

    private void LoadSettings()
    {
        settings ??= new Settings();

        settings.sensX = PlayerPrefs.GetFloat("SensX", 1f);
        settings.sensY = PlayerPrefs.GetFloat("SensY", 1f);
        settings.audioVolume = PlayerPrefs.GetFloat("Audio", 1f);
        settings.musicVolume = PlayerPrefs.GetFloat("Music", 1f);
        // currentLevel = PlayerPrefs.GetInt("Level", 1);
    }

    private void ApplySettings()
    {
        if (TryGetComponent<SettingsMenu>(out var settingsMenu))
            settingsMenu.ApplyAudioSettings();
    }

    private void OnApplicationQuit()
    {
        // PlayerPrefs.SetInt("Level", currentLevel);
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