using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    public AudioSource footstepSource;
    public AudioMixer audioMixer;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip prologueMusic;
    public AudioClip gameplayMusic;
    public AudioClip epilogueMusic;

    [Header("SFX")]
    public AudioClip clickSFX;
    // public AudioClip hoverSFX;
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

    private readonly HashSet<Button> buttonsWithClickSound = new();
    private float buttonScanTimer;


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
        SetInitialVolumes();
        SceneManager.sceneLoaded += OnSceneLoaded;
        RegisterButtonClickSounds();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        buttonScanTimer -= Time.unscaledDeltaTime;
        if (buttonScanTimer <= 0f)
        {
            buttonScanTimer = 0.5f;
            RegisterButtonClickSounds();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterButtonClickSounds();
    }

    void RegisterButtonClickSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (buttonsWithClickSound.Add(button))
            {
                button.onClick.AddListener(PlayClickSFX);
            }

            // if (button.GetComponent<HoverSoundTrigger>() == null)
            // {
            //     button.gameObject.AddComponent<HoverSoundTrigger>();
            // }
        }
    }

    void PlayClickSFX()
    {
        PlaySFX(clickSFX);
    }

    // public void PlayHoverSFX()
    // {
    //     PlaySFX(hoverSFX);
    // }

    void SetInitialVolumes()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);

        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource.clip == clip) return; // Avoid restarting the same music
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void PlayFootstepOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null || footstepSource == null) return;
        if (footstepSource.loop)
        {
            footstepSource.Stop();
            footstepSource.loop = false;
        }
        footstepSource.PlayOneShot(clip, volume);
    }

    public void StartFootstepLoop(AudioClip clip, float volume = 1f)
    {
        if (clip == null || footstepSource == null) return;
        if (footstepSource.isPlaying && footstepSource.loop && footstepSource.clip == clip) return;

        footstepSource.Stop();
        footstepSource.loop = true;
        footstepSource.clip = clip;
        footstepSource.volume = volume;
        footstepSource.Play();
    }

    public AudioClip GetCurrentFootstepClip()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isLevelScene = sceneName.IndexOf("Level", StringComparison.OrdinalIgnoreCase) >= 0;
        return isLevelScene ? footstepWoodSFX : footstepGrassSFX;
    }

    public void StopFootstepSFX()
    {
        if (footstepSource != null)
        {
            footstepSource.Stop();
        }
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
