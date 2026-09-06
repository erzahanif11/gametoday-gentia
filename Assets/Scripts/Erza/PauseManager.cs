using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public InputActionReference pauseActionReference;
    private bool isPauseMenuActive = false;

    void Update()
    {
        if (pauseActionReference.action.triggered)
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        isPauseMenuActive = !isPauseMenuActive;
        if (isPauseMenuActive)
        {
            Time.timeScale = 0f; // Pause the game
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
        }
    }

    public void ToggleSettings()
    {
        if (isPauseMenuActive)
        {
            settingsMenu.SetActive(true);
        }
    }

    public void BackToMainMenu()
    {
        if (isPauseMenuActive)
        {
            Time.timeScale = 1f; // Resume the game before going to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
