using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeScreen : MonoBehaviour
{
    public GameObject escScreen;

    private bool isPaused = false;

    public void PlayGame()
    {
        escScreen.SetActive(false);
    }
    public void OpenMenu()
    {
        SceneManager.LoadScene(1);
    }
    public void OptionsLoad()
    {
        SceneManager.LoadScene(2);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
