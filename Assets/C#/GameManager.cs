using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public static bool isGameOver = false;

    [SerializeField] private Camera loaderCamera;
    private string currentSceneName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //load main menu scene
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

    //public void OnApplicationQuit()
    //{
    //    //save the game data - records, unlocked levels, settings
    //}
}
