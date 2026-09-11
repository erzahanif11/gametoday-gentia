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
        if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
        }
        else if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        if (audioManager == null)
        {
            return;
        }

        masterVolumeSlider.value = audioManager.masterVolume;
        musicVolumeSlider.value = audioManager.musicVolume;
        sfxVolumeSlider.value = audioManager.sfxVolume;
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
