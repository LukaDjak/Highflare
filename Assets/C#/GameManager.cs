using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public static bool isGameOver = false;

    [SerializeField] private Camera loaderCamera;
    private string currentSceneName;

    public static Settings settings;

    [HideInInspector] public int currentLevel = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        settings ??= new Settings();

        settings.sensX = PlayerPrefs.GetFloat("SensX", 1f);
        settings.sensY = PlayerPrefs.GetFloat("SensY", 1f);
        settings.audioVolume = PlayerPrefs.GetFloat("Audio", 1f);
        settings.musicVolume = PlayerPrefs.GetFloat("Music", 1f);
        //currentLevel = PlayerPrefs.GetInt("Level", 1);

        ApplySettings();

        //load main menu scene
        //LoadScene("MainMenu");
    }

    public void LoadScene(string loadSceneName, string unloadSceneName = null)
    {
        if (!string.IsNullOrEmpty(unloadSceneName) || !string.IsNullOrEmpty(currentSceneName))
        {
            loaderCamera.gameObject.SetActive(true);
            SceneManager.UnloadSceneAsync(unloadSceneName ?? currentSceneName);
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);

        loadOperation.completed += (operation) =>
        {
            if (loaderCamera != null)
                loaderCamera.gameObject.SetActive(false);
        };
        currentSceneName = loadSceneName;
    }

    public void OnApplicationQuit()
    {
        //add: save the game
        //PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetFloat("SensX", settings.sensX);
        PlayerPrefs.SetFloat("SensY", settings.sensY);
        PlayerPrefs.SetFloat("Audio", settings.audioVolume);
        PlayerPrefs.SetFloat("Music", settings.musicVolume);
        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        if (TryGetComponent<SettingsMenu>(out var settingsMenu))
            settingsMenu.ApplyAudioSettings();
    }
}

public class Settings
{
    public float sensX, sensY;
    public float audioVolume, musicVolume;
}