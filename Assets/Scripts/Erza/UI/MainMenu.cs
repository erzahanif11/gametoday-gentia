using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsMenu;
    private bool isSettingsMenuActive = false;

    public void PlayGame()
    {
        if (isSettingsMenuActive)
        {
            return;
        }
        FadeTransition.Instance.TransitionToScene("Prologue");
    }

    public void ToggleSettings()
    {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        isSettingsMenuActive = !isSettingsMenuActive;
    }

    public void QuitGame()
    {
        if (isSettingsMenuActive)
        {
            return;
        }
        Application.Quit();
    }
}
