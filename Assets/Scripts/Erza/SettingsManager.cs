using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioManager audioManager;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        masterVolumeSlider.value = audioManager.masterVolume;
        musicVolumeSlider.value = audioManager.musicVolume;

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    void SetMasterVolume(float volume)
    {
        audioManager.SetMasterVolume(volume);
    }

    void SetMusicVolume(float volume)
    {
        audioManager.SetMusicVolume(volume);
    }

    void SetSfxVolume(float volume)
    {
        audioManager.SetSfxVolume(volume);
    }
}
