using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelClear : MonoBehaviour
{
    public void NextLevel()
    {
        FadeTransition.Instance.TransitionToScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RestartLevel()
    {
        FadeTransition.Instance.TransitionToScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        FadeTransition.Instance.TransitionToScene("MainMenu");
    }
}
