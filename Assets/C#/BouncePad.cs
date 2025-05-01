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
            other.collider.GetComponent<Rigidbody>().AddForce(useVelocity ? other.transform.up * bouncerStrength / 1.5f * other.collider.GetComponent<OldPlayerMovement>().ms.moveSpeed : 12f * bouncerStrength * other.transform.up, ForceMode.Impulse);
            other.collider.GetComponent<Rigidbody>().AddForce(2f * bouncerStrength * other.transform.forward, ForceMode.Impulse);
        }
    }
}