using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    private Animator animator;

    private void Start() => animator = GetComponent<Animator>();
    public void ShowObjective() => animator.SetTrigger("ShowObjective");
}
