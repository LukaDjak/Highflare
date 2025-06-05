using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int bouncerStrength;
    [SerializeField] private bool useVelocity; //basically multiplies force based on player's velocity - gets launched higher when falling on boundepad from larger distance
    [SerializeField] private AudioClip bouncerClip;

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            if (bouncerClip)
                SoundManager.instance.PlaySound(bouncerClip, transform.position);

            if (other.collider.TryGetComponent<YT_PlayerMovement>(out var player))
            {
                Vector3 bounceDirection = transform.up.normalized;
                float forceAmount = 10f * bouncerStrength;
                player.ApplyExternalForce(bounceDirection * forceAmount, 0.4f);
            }
        }
    }
}