using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionRoom : MonoBehaviour
{
    private Door startDoor;
    private Door endDoor;
    private Timer timer;

    private bool isTransitioning;
    private bool shouldTransition = false;
    private bool startingGame = true;
    private bool shouldCloseStartDoor = false;

    private Coroutine transitionCoroutine;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        isTransitioning = true;

        startDoor.ToggleDoor(); // Open start door initially
        timer = FindObjectOfType<Timer>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reacquire references
        startDoor = GameObject.Find("StartDoor").GetComponent<Door>();
        endDoor = GameObject.Find("EndDoor").GetComponent<Door>();
        timer = FindObjectOfType<Timer>();

        if (shouldCloseStartDoor && startDoor != null)
        {
            startDoor.ToggleDoor(); // Close start door
            shouldCloseStartDoor = false;

            if (timer != null)
                timer.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!startingGame && shouldTransition && transitionCoroutine == null)
            transitionCoroutine = StartCoroutine(HandleLoadingLevel());
        else if (startingGame)
            startingGame = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isTransitioning && transitionCoroutine != null)
            StopCoroutine(HandleLoadingLevel());

        OnRoomExit();
    }

    private void OnRoomExit()
    {
        startDoor.ToggleDoor(); // Close the start door
        isTransitioning = false;

        if (timer != null)
            timer.enabled = true;
    }

    public void OnCrownCollected()
    {
        if (timer != null)
            timer.enabled = false;

        shouldTransition = true;

        // TODO: Trigger door open cutscene
        endDoor.ToggleDoor(); // Open end door
    }

    private IEnumerator HandleLoadingLevel()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(1f);

        endDoor.ToggleDoor(); //close end door
        GameManager.instance.currentLevel++;

        yield return new WaitForSeconds(2f);

        shouldCloseStartDoor = true;

        // 🟡 Save player's transform before unloading
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
        }

        GameManager.instance.LoadScene(
            $"Level{GameManager.instance.currentLevel}",
            $"Level{GameManager.instance.currentLevel - 1}"
        );

        // 🟢 Restore position on new player after loading
        GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");
        if (newPlayer != null)
            newPlayer.transform.SetPositionAndRotation(savedPlayerPosition, savedPlayerRotation);

        yield return new WaitForSeconds(1f); // Wait for scene load to complete

        // The door and timer are reset in OnSceneLoaded
        isTransitioning = false;
        shouldTransition = false;
        transitionCoroutine = null;
    }
}