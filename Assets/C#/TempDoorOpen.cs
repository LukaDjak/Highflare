using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TempDoorOpen : MonoBehaviour
{
    public UnityEvent openStartDoor;

    private void Start()
    {
        openStartDoor.Invoke();
    }
}
