using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject confirmationMenu;
    public InputActionReference pauseActionReference;
    private bool isPauseMenuActive = false;
    private bool isPopUpActive = false;

    void Update()
    {
        if (pauseActionReference.action.triggered && !isPopUpActive && !TutorialManager.IsTutorialOpen && !PopupManager.IsPopupOpen)
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
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        isPopUpActive = settingsMenu.activeSelf;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; // Resume the game before going to main menu
        FadeTransition.Instance.TransitionToScene("MainMenu");
    }

    public void ToggleConfirmationMenu()
    {
        confirmationMenu.SetActive(!confirmationMenu.activeSelf);
        isPopUpActive = confirmationMenu.activeSelf;
    }
}
