using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
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
        //add scene transition + loaderscene in the future
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        //add scene transition + loaderscene in the future
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowWinScreen()
    {
        GameManager.isGameOver = true;
        Time.timeScale = 0.5f;
        background.SetActive(true);
        winMenu.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void AdvanceToNextLevel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}