using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Door door;
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            door.ToggleDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggered = false;
        door.ToggleDoor();
    }
}