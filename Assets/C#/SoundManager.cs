using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Settings")]
    [SerializeField] private int poolSize = 10;

    private List<AudioSource> audioSources;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        //create a pool of AudioSources
        audioSources = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new("PooledAudioSource_" + i);
            go.transform.parent = transform;
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = false; //this allows audio to be paused when the game is paused
            audioSources.Add(source);
        }
    }

    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f, Transform parent = null)
    {
        if (clip == null) return;

        //prevent duplicate sound nearby
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying && source.clip == clip)
            {
                float distance = Vector3.Distance(source.transform.position, position);
                if (distance < .001f)
                    return; //skip playing the same sound too close
            }
        }

        AudioSource availableSource = GetAvailableSource();
        if (availableSource == null) return;

        if (parent != null)
        {
            availableSource.transform.SetParent(parent);
            availableSource.transform.localPosition = Vector3.zero;
        }
        else
        {
            availableSource.transform.position = position;
            availableSource.transform.parent = null;
        }

        availableSource.clip = clip;
        availableSource.volume = volume;
        availableSource.pitch = pitch;
        availableSource.spatialBlend = spatialBlend;
        availableSource.Play();

        if(parent != transform)
            StartCoroutine(DetachAfterPlay(availableSource, clip.length));
    }


    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
                return source;
        }
        Debug.LogWarning("No free AudioSources available, increase the pool size!");
        return null;
    }

    private IEnumerator DetachAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        //reset back under SoundManager
        if (source != null)
        {
            source.transform.SetParent(transform);
            source.clip = null; //optional: clear clip to mark as "available" faster
        }
    }
}