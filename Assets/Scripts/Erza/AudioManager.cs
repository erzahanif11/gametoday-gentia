using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

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
}
