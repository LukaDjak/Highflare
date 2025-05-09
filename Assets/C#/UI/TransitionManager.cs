using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;

    [Tooltip("List of all transition trigger names from the Animator")]
    [SerializeField] private List<string> transitionTriggers = new();

    public static TransitionManager instance;

    private string lastTrigger;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void DoTransition(string transitionName = null)
    {
        string triggerToUse = transitionName;

        if (string.IsNullOrEmpty(triggerToUse))
        {
            //pick a random trigger that is different from the last one
            do
            {
                triggerToUse = transitionTriggers[Random.Range(0, transitionTriggers.Count)];
            } while (triggerToUse == lastTrigger && transitionTriggers.Count > 1);
        }

        transitionAnimator.SetTrigger(triggerToUse);
        lastTrigger = triggerToUse;
    }
}