using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Vector3 openPositionOffset;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 3f;

    private bool isOpen;
    private Vector3 closedPosition;
    private Coroutine moveCoroutine;

    private void Start()
    {
        closedPosition = transform.position;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        Vector3 targetPosition = isOpen ? closedPosition + openPositionOffset : closedPosition;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(targetPosition));

        if (isOpen && autoClose)
            StartCoroutine(AutoCloseAfterDelay());
    }

    private IEnumerator MoveDoor(Vector3 target)
    {
        Vector3 start = transform.position;
        float duration = Vector3.Distance(start, target) / openSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            //ease In-Out using SmoothStep
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        ToggleDoor(); // close
    }
}