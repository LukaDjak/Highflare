using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void NewGame() => StartCoroutine(LoadWithTransition("MainScene", "Level1"));
    public void LoadEndless() => StartCoroutine(LoadWithTransition("MainScene", "Endless"));
    public void LoadTesting() => StartCoroutine(LoadWithTransition("MainScene", "Movement"));
    public void QuitGame() => Application.Quit();

    public void TogglePanel(int index)
    {
        for (int i = 0; i < panels.Count; i++)
            panels[i].SetActive(i == index);
    }

    private IEnumerator LoadWithTransition(string baseScene, string levelScene)
    {
        TransitionManager.instance.DoTransition();
        yield return new WaitForSeconds(.45f);

        GameManager.instance.LoadScene(baseScene);
        GameManager.instance.LoadScene(levelScene, "MainMenu");
    }
}