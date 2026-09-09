using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    public AudioMixer audioMixer;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip prologueMusic;
    public AudioClip gameplayMusic;
    public AudioClip epilogueMusic;

    [Header("SFX")]
    public AudioClip clickSFX;
    public AudioClip angelLandingSFX;
    public AudioClip footstepGrassSFX;
    public AudioClip footstepWoodSFX;
    public AudioClip leverInteractionSFX;
    public AudioClip openDoorSFX;
    public AudioClip platformAppearSFX;
    public AudioClip platformDisappearSFX;
    public AudioClip puzzleCompleteSFX;
    public AudioClip spiritCaptured;

    [Header("Ambient")]
    public AudioClip softWindAmbient;

    [Header("Volume Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public static AudioManager Instance { get; private set; }

    void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource.clip == clip) return; // Avoid restarting the same music
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
}
