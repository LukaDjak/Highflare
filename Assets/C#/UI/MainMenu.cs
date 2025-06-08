using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private List<GameObject> panels = new();

    private void Start() => continueButton.interactable = GameManager.instance.currentLevel >= 2;

    public void Continue()
    {
        string levelName = $"Level{GameManager.instance.currentLevel}";
        StartCoroutine(LoadWithTransition("MainScene", levelName));
    }

    public void NewGame()
    {
        GameManager.instance.currentLevel = 1;
        StartCoroutine(LoadWithTransition("MainScene", "Level1"));
    }
    public void LoadEndless() => StartCoroutine(LoadWithTransition("MainScene", "Endless"));
    public void QuitGame() => Application.Quit();

    public void TogglePanel(int index)
    {
        for (int i = 0; i < panels.Count; i++)
            panels[i].SetActive(i == index);
    }

    private IEnumerator LoadWithTransition(string baseScene, string levelScene)
    {
        if (GameManager.justEnteredGame)
            TransitionManager.instance.DoTransition("Fade"); //fade transition for cutscene, other code handled in TransitionRoom
        else
            TransitionManager.instance.DoTransition();
            yield return new WaitForSeconds(.45f);

        GameManager.instance.LoadScene(baseScene);

        //load level scene and wait until it's fully loaded
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelScene, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(levelScene);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);
        SceneManager.UnloadSceneAsync("MainMenu");
    }
}