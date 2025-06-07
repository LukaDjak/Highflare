using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WindSound : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody playerRb;

    [Header("Wind Settings")]
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float volumeSmoothSpeed = 3f;

    private AudioSource windSource;
    private float targetVolume;

    private void Start() => windSource = GetComponent<AudioSource>();
    private void Update()
    {
        float speed = playerRb.velocity.magnitude;
        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        targetVolume = t * maxVolume;
        windSource.volume = Mathf.Lerp(windSource.volume, targetVolume, Time.deltaTime * volumeSmoothSpeed);
    }
}