using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundtrack : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> soundtracks;

    private Queue<AudioClip> shuffleQueue;
    private AudioClip lastPlayed;

    private void Start()
    {
        PrepareShuffleQueue();
        PlayNextTrack();
    }


    private void PrepareShuffleQueue()
    {
        //create a shuffled queue from the soundtracks
        List<AudioClip> shuffledTracks = new(soundtracks);
        shuffledTracks.Remove(lastPlayed); //remove the last played track (if exists)
        ShuffleList(shuffledTracks);

        shuffleQueue = new Queue<AudioClip>(shuffledTracks);

        //re-add the last played track to the end of the shuffle to avoid immediate repeat
        if (lastPlayed != null && soundtracks.Count > 1)
            shuffleQueue.Enqueue(lastPlayed);
    }

    private void ShuffleList(List<AudioClip> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void PlayNextTrack()
    {
        if (shuffleQueue == null || shuffleQueue.Count == 0)
            PrepareShuffleQueue();

        //get the next track and play it
        AudioClip nextTrack = shuffleQueue.Dequeue();
        lastPlayed = nextTrack;

        audioSource.clip = nextTrack;
        audioSource.Play();

        //schedule the next track after the current one finishes
        StartCoroutine(WaitForTrackToEnd(nextTrack.length));
    }

    private IEnumerator WaitForTrackToEnd(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        PlayNextTrack();
    }
}