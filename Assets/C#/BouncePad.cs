using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField, Range(1, 5)] private int bouncerStrength;
    [SerializeField] private bool useVelocity; //basically multiplies force based on player's velocity - gets launched higher when falling on boundepad from larger distance
    [SerializeField] private AudioClip bouncerClip;

    //private void OnCollisionEnter(Collision other)
    //{
    //    if (other.collider.CompareTag("Player"))
    //    {
    //        if (bouncerClip)
    //            SoundManager.instance.PlaySound(bouncerClip, transform.position);
    //        other.collider.GetComponent<Rigidbody>().AddForce(useVelocity ? other.transform.up * bouncerStrength / 1.5f * other.collider.GetComponent<OldPlayerMovement>().ms.moveSpeed : 12f * bouncerStrength * other.transform.up, ForceMode.Impulse);
    //        other.collider.GetComponent<Rigidbody>().AddForce(2f * bouncerStrength * other.transform.forward, ForceMode.Impulse);
    //    }
    //}

    //private void OnCollisionEnter(Collision other)
    //{
    //    if (other.collider.CompareTag("Player"))
    //    {
    //        Rigidbody rb = other.collider.GetComponent<Rigidbody>();
    //        var movement = other.collider.GetComponent<YT_PlayerMovement>();

    //        if (rb == null || movement == null) return;

    //        // Play sound
    //        if (bouncerClip)
    //            SoundManager.instance.PlaySound(bouncerClip, transform.position);

    //        // Calculate launch direction based on pad's orientation
    //        Vector3 launchDirection = transform.up;

    //        // Calculate launch force
    //        float forceMagnitude = useVelocity
    //            ? bouncerStrength / 1.5f * movement.GetMoveSpeed()
    //            : 12f * bouncerStrength;

    //        // Launch in the pad's up direction
    //        rb.AddForce(launchDirection * forceMagnitude, ForceMode.Impulse);

    //        // Optionally add forward push (e.g., for directional bounce pads)
    //        rb.AddForce(transform.forward * (2f * bouncerStrength), ForceMode.Impulse);
    //    }
    //}

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