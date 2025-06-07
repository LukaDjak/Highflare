using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneScriptSet
{
    public string sceneName;
    public MonoBehaviour[] scriptsToEnable;
}

public class MovementToggler : MonoBehaviour
{
    [Header("All scripts to manage (disabled on start)")]
    [SerializeField] private MonoBehaviour[] allManagedScripts;

    [Header("Scripts to enable per scene")]
    [SerializeField] private SceneScriptSet[] sceneScriptSets;

    private void Start()
    {
        Scene currentScene = SceneManager.GetSceneAt(2);
        if (currentScene != null)
            ToggleScriptsForScene(currentScene.name);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ToggleScriptsForScene(scene.name);

    private void ToggleScriptsForScene(string sceneName)
    {
        // Disable all scripts
        foreach (var script in allManagedScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        // Find matching scene config and enable the listed scripts
        foreach (var set in sceneScriptSets)
        {
            if (set.sceneName == sceneName)
            {
                foreach (var script in set.scriptsToEnable)
                {
                    if (script != null)
                        script.enabled = true;
                }
                return;
            }
        }
    }
}