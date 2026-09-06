using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPauseMenuActive = false;

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
}
