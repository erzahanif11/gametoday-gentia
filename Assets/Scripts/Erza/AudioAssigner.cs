using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioAssigner : MonoBehaviour
{
    public AudioManager audioManager;

    void Awake()
    {
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            audioManager.StopMusic();
            audioManager.PlayMusic(audioManager.mainMenuMusic);
        }
        else if (scene.name == "Prologue")
        {
            audioManager.StopMusic();
            audioManager.PlayMusic(audioManager.prologueMusic);
        }
        else if (scene.name == "Epilogue")
        {
            audioManager.StopMusic();
            audioManager.PlayMusic(audioManager.epilogueMusic);
        }
        else if(scene.name == "Level1")
        {
            audioManager.PlayMusic(audioManager.gameplayMusic);
        }
    }
}
