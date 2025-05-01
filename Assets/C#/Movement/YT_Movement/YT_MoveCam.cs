using UnityEngine;

public class YT_MoveCam : MonoBehaviour
{
    [SerializeField] private Transform camPos;

    private void Update() => transform.position = camPos.position;
}