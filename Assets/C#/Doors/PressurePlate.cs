using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Door door;
    private bool triggered;
    private string currentObject;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            currentObject = other.name;
            door.ToggleDoor();
            GetComponent<ButtonFeedback>().Press();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(triggered && other.name == currentObject)
        {
            triggered = false;
            currentObject = null;
            door.ToggleDoor();
            GetComponent<ButtonFeedback>().Release();
        }
    }
}