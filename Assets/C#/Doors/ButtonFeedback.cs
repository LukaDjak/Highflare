using System.Collections;
using UnityEngine;

public class ButtonFeedback : MonoBehaviour
{
    [SerializeField] private Vector3 pressedOffset = new(0, -0.05f, 0);
    [SerializeField] private float pressSpeed = 10f;
    [SerializeField] private bool autoRelease = false;
    [SerializeField] private float releaseDelay = .2f;

    private Vector3 originalPos;
    private bool isPressed;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Press()
    {
        if (isPressed) return;
        isPressed = true;
        StopAllCoroutines();
        if (autoRelease)
            StartCoroutine(PressThenRelease());
        else
            StartCoroutine(MoveTo(originalPos + pressedOffset));
    }

    public void Release()
    {
        if (!isPressed) return;
        isPressed = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(originalPos));
    }

    private IEnumerator PressThenRelease()
    {
        yield return StartCoroutine(MoveTo(originalPos + pressedOffset));
        yield return new WaitForSeconds(releaseDelay);
        yield return StartCoroutine(MoveTo(originalPos));
        isPressed = false;
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * pressSpeed);
            yield return null;
        }

        transform.localPosition = target;
    }
}
