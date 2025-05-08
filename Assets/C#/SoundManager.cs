using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Settings")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private AudioMixerGroup audioMixer;

    private readonly List<AudioSource> audioSources = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        CreateAudioPool();
    }

    private void CreateAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var sourceGO = new GameObject($"PooledAudioSource_{i}");
            sourceGO.transform.SetParent(transform);
            var source = sourceGO.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = false;
            source.outputAudioMixerGroup = audioMixer;
            audioSources.Add(source);
        }
    }

    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f, Transform parent = null)
    {
        if (clip == null) return;

        //prevent duplicate sound nearby
        foreach (var source in audioSources)
        {
            if (source.isPlaying && source.clip == clip &&
                Vector3.Distance(source.transform.position, position) < 0.001f)
                return;
        }

        var sourceToUse = GetAvailableSource();
        if (sourceToUse == null) return;

        //set position and parenting
        if (parent != null)
        {
            sourceToUse.transform.SetParent(parent);
            sourceToUse.transform.localPosition = Vector3.zero;
        }
        else
        {
            sourceToUse.transform.SetParent(null);
            sourceToUse.transform.position = position;
        }

        sourceToUse.clip = clip;
        sourceToUse.volume = volume;
        sourceToUse.pitch = pitch;
        sourceToUse.spatialBlend = spatialBlend;
        sourceToUse.Play();
        StartCoroutine(DetachAfterPlay(sourceToUse));
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
                return source;
        }
        Debug.LogWarning("No free AudioSources available — consider increasing pool size.");
        return null;
    }

    private IEnumerator DetachAfterPlay(AudioSource source)
    {
        yield return new WaitWhile(() => source != null && source.isPlaying);

        if (source != null)
        {
            source.transform.SetParent(transform);
            source.clip = null;
        }
    }
}