using UnityEngine;

public class AudioFollower : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform newTarget) => target = newTarget;

    private void LateUpdate()
    {
        if (target != null)
            transform.position = target.position;
    }
}