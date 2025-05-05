using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionRoom : MonoBehaviour
{
    [Header("References")]
    public Door startDoor;
    public Door endDoor;

    //open start door 1 second later after Start()
    //when player leaves the room, start the timer and close the door
    //bug fix - if player manages to stay in the room after closing starting doors, kill him

    //on crown collected, show the cutscene of opening end door (idk how lmao)
    //when player enters the room after collecting the crown, close the door 1 second later
    //unload previous and load next level
    //wait 1 more second and open start door for the next level
    //repeat :)
}