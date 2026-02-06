using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneScriptSet
{
    public string sceneName;
    public MonoBehaviour[] scriptsToEnable;
}

public class MovementManager : MonoBehaviour
{
    [Header("All scripts to manage (disabled on start)")]
    [SerializeField] private MonoBehaviour[] allManagedScripts;

    [Header("Scripts to enable per scene")]
    [SerializeField] private SceneScriptSet[] sceneScriptSets;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        if (!GameManager.justEnteredGame)
            ApplySceneScriptSet(SceneManager.GetSceneAt(2));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GameManager.justEnteredGame)
            ApplySceneScriptSet(scene);
    }

    private void ApplySceneScriptSet(Scene scene)
    {
        if (!scene.IsValid())
            return;

        string sceneName = scene.name;

        //all scenes needed in this if are in sceneScriptSets value,
        //just need a way to write it correctly here
        if (sceneName != "Level1" && sceneName != "Level2" && sceneName != "Prototype") 
        {
            EnableAllManagedScripts();
            return;
        }

        DisableAllManagedScripts();

        foreach (var set in sceneScriptSets)
        {
            if (set.sceneName == sceneName)
            {
                foreach (var script in set.scriptsToEnable)
                {
                    if (script != null)
                        script.enabled = true;
                }
                break;
            }
        }
    }

    //disables movement during cutscene - called in Timeline
    public void DisableAll() => DisableAllManagedScripts();

    //enables movement after cutscene - called in Timeline
    public void EnableAll() => ApplySceneScriptSet(SceneManager.GetSceneAt(2));

    private void EnableAllManagedScripts()
    {
        foreach (var script in allManagedScripts)
        {
            if (script != null)
                script.enabled = true;
        }
    }

    private void DisableAllManagedScripts()
    {
        foreach (var script in allManagedScripts)
        {
            if (script != null)
                script.enabled = false;
        }
    }
}