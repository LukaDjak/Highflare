using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Void : MonoBehaviour
{
    [Header("Death Settings")]
    public float deathYLevel = -20f; //the Y position where the player dies
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(CheckPlayerFall());
    }

    IEnumerator CheckPlayerFall()
    {
        while (true)
        {
            if (player.position.y < deathYLevel)
            {
                FindObjectOfType<PauseMenu>().ShowGameOver();
                yield break; //stop the coroutine after restarting the scene
            }
            yield return new WaitForSeconds(0.1f); //check every 0.1 seconds - better than checking in Update()
        }
    }
}