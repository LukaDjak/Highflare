using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionRoom : MonoBehaviour
{
    [Header("References")]
    public Door startDoor;
    public Door endDoor;

    private Timer timer;
    private Coroutine transitionCoroutine;

    private bool isTransitioning = true;
    private bool shouldTransition = false;
    private bool startingGame = true;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        timer = FindObjectOfType<Timer>();
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() => startDoor.ToggleDoor(); // Open start door at beginning

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        timer = FindObjectOfType<Timer>();
        isTransitioning = true;
        startDoor.ToggleDoor(); // Open door again for new level
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (shouldTransition && !startingGame && transitionCoroutine == null)
            transitionCoroutine = StartCoroutine(HandleLevelTransition());
        else if (startingGame)
            startingGame = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isTransitioning && transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        OnRoomExit();
    }

    private void OnRoomExit()
    {
        startDoor.ToggleDoor(); // Close start door
        isTransitioning = false;

        if (timer != null)
            timer.enabled = true;
    }

    public void OnCrownCollected()
    {
        if (timer != null)
            timer.enabled = false;

        shouldTransition = true;
        endDoor.ToggleDoor(); // Open end door (eventually replaced with a cutscene)
    }

    private IEnumerator HandleLevelTransition()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(1f);
        endDoor.ToggleDoor(); // Close end door

        GameManager.instance.currentLevel++;
        Debug.Log($"Loading Level {GameManager.instance.currentLevel}");

        yield return new WaitForSeconds(2f);

        GameManager.instance.LoadScene(
            $"Level{GameManager.instance.currentLevel}",
            $"Level{GameManager.instance.currentLevel - 1}"
        );

        yield return new WaitForSeconds(1f); // Let things settle

        isTransitioning = false;
        shouldTransition = false;
        transitionCoroutine = null;
    }
}