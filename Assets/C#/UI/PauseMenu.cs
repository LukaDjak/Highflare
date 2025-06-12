using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private AudioClip deathClip;

    [Header("UI Panels")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private AudioMixer audioMixer;

    private bool isPaused = false;
    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.isGameOver = false;
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
        if(FindObjectOfType<TransitionRoom>().timeline.activeInHierarchy) return; //do not allow to pause the game during the cutscene :)

        Time.timeScale = 0f;
        audioMixer.SetFloat("GameSound", -80f);
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
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(GameManager.settings.audioVolume, 0.001f, 1f)) * 20f);
        isPaused = false;
        background.SetActive(false);
        pauseMenu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GameOver()
    {
        GameManager.isGameOver = true;
        SoundManager.instance.PlaySound(deathClip, Vector3.zero, 1, 1, 0);
        Time.timeScale = 0.5f; //slow down time
        background.SetActive(true);
        gameOverMenu.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(GameManager.settings.audioVolume, 0.001f, 1f)) * 20f);
        isPaused = false;
        StartCoroutine(RestartLevelRoutine());
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(GameManager.settings.audioVolume, 0.001f, 1f)) * 20f);
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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