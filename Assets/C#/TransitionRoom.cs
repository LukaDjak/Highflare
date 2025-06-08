using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionRoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Door startDoor;
    [SerializeField] private Door endDoor;
    [SerializeField] private GameObject timeline;

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

    private void Start()
    {
        if (GameManager.justEnteredGame)
        {
            timeline.SetActive(true);
            GameManager.justEnteredGame = false;
        }
        else
        {
            timeline.SetActive(false);
            OpenStartDoorIfClosed();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        timer = FindObjectOfType<Timer>();
        isTransitioning = true;

        if (!timeline.activeInHierarchy)
            OpenStartDoorIfClosed();
    }

    private void OpenStartDoorIfClosed()
    {
        if (!startDoor.isOpen)
        {
            startDoor.ToggleDoor();
            StartCoroutine(ShowObjective());
        }
    }

    public void ObjectiveSignal() => StartCoroutine(ShowObjective());

    private IEnumerator ShowObjective()
    {
        yield return new WaitForSeconds(1f);

        LevelObjective objective = FindObjectOfType<LevelObjective>();
        if (objective != null)
            objective.ShowObjective();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (shouldTransition && !startingGame && transitionCoroutine == null)
        {
            transitionCoroutine = StartCoroutine(HandleLevelTransition());
        }
        else if (startingGame)
        {
            startingGame = false;
        }
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
        endDoor.ToggleDoor(); // Open end door
    }

    private IEnumerator HandleLevelTransition()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(1f);
        endDoor.ToggleDoor(); // Close end door

        if (GameManager.instance.currentLevel == 5)
        {
            FindObjectOfType<PauseMenu>().GoToMainMenu();
            yield break;
        }

        GameManager.instance.currentLevel++;
        Debug.Log($"Loading Level {GameManager.instance.currentLevel}");

        yield return new WaitForSeconds(2f);

        FindObjectOfType<PickUpController>().DropAndDestroyWeapon();

        GameManager.instance.LoadScene(
            $"Level{GameManager.instance.currentLevel}",
            $"Level{GameManager.instance.currentLevel - 1}"
        );

        yield return new WaitForSeconds(1f);

        isTransitioning = false;
        shouldTransition = false;
        transitionCoroutine = null;
    }
}