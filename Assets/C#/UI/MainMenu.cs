using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private List<GameObject> panels = new();

    private void Start() => continueButton.interactable = GameManager.instance.currentLevel >= 2;

    public void NewGame()
    {
        GameManager.instance.LoadScene("MainScene");
        GameManager.instance.LoadScene("Level1", "MainMenu");
    }

    public void Continue()
    {
        string levelName = $"Level{GameManager.instance.currentLevel}";
        GameManager.instance.LoadScene("MainScene");
        GameManager.instance.LoadScene(levelName, "MainMenu");
    }

    public void LoadEndless()
    {
        GameManager.instance.LoadScene("MainScene");
        GameManager.instance.LoadScene("Endless", "MainMenu");
    }

    public void LoadTesting()
    {
        GameManager.instance.LoadScene("MainScene");
        GameManager.instance.LoadScene("Movement", "MainMenu");
    }

    public void QuitGame() => Application.Quit();

    public void TogglePanel(int index)
    {
        for (int i = 0; i < panels.Count; i++)
            panels[i].SetActive(i == index);
    }
}