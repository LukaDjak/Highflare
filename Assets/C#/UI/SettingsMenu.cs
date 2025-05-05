using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private TMP_Text sensXText, sensYText, audioText, musicText;

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
        audioMixer.SetFloat("GameSound", Mathf.Log10(level) * 20f);
        GameManager.settings.audioVolume = level;
    }
    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(level) * 20f);
        GameManager.settings.musicVolume = level;
    }

    public void SetXSensitivity(float value) => GameManager.settings.sensX = value;
    public void SetYSensitivity(float value) => GameManager.settings.sensY = value;

    //handle saving and loading settings, please
}