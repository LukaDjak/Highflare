using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRefs : MonoBehaviour
{
    [Header("Basic References")]
    public Rigidbody rb;
    public PlayerMovement pm;
    public PlayerCam cam;
    public Transform orientation;

    [Header("Optional References")]
    public Dash dash;
}
