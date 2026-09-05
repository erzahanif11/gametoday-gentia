using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsMenu;

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Prologue");
    }

    public void ToggleSettings()
    {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
