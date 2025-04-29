using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CrosshairManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private Sprite crosshair;
    private Coroutine fillRoutine;

    public static CrosshairManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Set the main crosshair sprite (icon).
    /// </summary>
    public void SetCrosshair(Sprite sprite)
    {
        crosshairImage.sprite = sprite;
        crosshairImage.enabled = sprite != null;
    }

    public void ResetCrosshair()
    {
        crosshairImage.sprite = crosshair;
        crosshairImage.enabled = true;
    }
    /// <summary>
    /// Starts a timed fill over the given duration (e.g., for reloading).
    /// </summary>
    public void StartTimedFill(float duration)
    {
        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillRoutine(duration));
    }

    private IEnumerator FillRoutine(float duration)
    {
        float elapsed = 0f;
        fillImage.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        //reset it
        fillImage.fillAmount = 0f;
        fillRoutine = null;
    }
}