using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private TMP_Text sensXText, sensYText, audioText, musicText;
    [SerializeField] private Slider sensXSlider, sensYSlider, audioSlider, musicSlider;

    private void Start()
    {
        sensXSlider.value = GameManager.settings.sensX;
        sensYSlider.value = GameManager.settings.sensY;
        audioSlider.value = GameManager.settings.audioVolume;
        musicSlider.value = GameManager.settings.musicVolume;
    }

    private void Update()
    {
        sensXText.text = $"{GameManager.settings.sensX:F1}X";
        sensYText.text = $"{GameManager.settings.sensY:F1}X";
        audioText.text = $"{GameManager.settings.audioVolume * 100:F0}%";
        musicText.text = $"{GameManager.settings.musicVolume * 100:F0}%";
    }

    //called on sliders
    public void SetSoundVolume(float level)
    {
        GameManager.settings.audioVolume = level;
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(level, 0.001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("Audio", level);
    }

    public void SetMusicVolume(float level)
    {
        GameManager.settings.musicVolume = level;
        audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Clamp(level, 0.001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("Music", level);
    }

    public void SetXSensitivity(float value)
    {
        GameManager.settings.sensX = value;
        PlayerPrefs.SetFloat("SensX", value);
    }

    public void SetYSensitivity(float value)
    {
        GameManager.settings.sensY = value;
        PlayerPrefs.SetFloat("SensY", value);
    }

    public void ApplyAudioSettings()
    {
        // Set AudioMixer volumes based on current GameManager settings
        audioMixer.SetFloat("GameSound", Mathf.Log10(Mathf.Clamp(GameManager.settings.audioVolume, 0.001f, 1f)) * 20f);
        audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Clamp(GameManager.settings.musicVolume, 0.001f, 1f)) * 20f);
    }
}