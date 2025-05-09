using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject winMenu;
    private AudioListener audioListener;

    private bool isPaused = false;
    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.isGameOver = false;
        audioListener = Camera.main.GetComponent<AudioListener>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.isGameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        isPaused = true;
        background.SetActive(true);
        pauseMenu.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (GameManager.isGameOver) return;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        background.SetActive(false);
        pauseMenu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowGameOver()
    {
        GameManager.isGameOver = true;
        Time.timeScale = 0.5f; //slow down time
        background.SetActive(true);
        gameOverMenu.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        StartCoroutine(RestartLevelRoutine());
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        StartCoroutine(GoToMainMenuRoutine());
    }
    private IEnumerator RestartLevelRoutine()
    {
        TransitionManager.instance.DoTransition();
        yield return new WaitForSeconds(.45f);

        GameManager.instance.LoadScene("MainScene", "MainScene");

        string scene = SceneManager.GetSceneAt(2).name;
        GameManager.instance.LoadScene(scene, scene);
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        TransitionManager.instance.DoTransition();
        yield return new WaitForSeconds(.45f);

        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(2).buildIndex);
        GameManager.instance.LoadScene("MainMenu", "MainScene");
    }
}