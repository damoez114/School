using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // Reference to your Pause Menu UI Panel
    [SerializeField] private GameObject pauseMenuUI;

    public static bool isPaused = false;

    // This method will be triggered by your UI Button
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;          // Freezes the game simulation
        isPaused = true;
        pauseMenuUI.SetActive(true);  // Displays the pause menu screen
        GameState.IsPlacing = true;

        // Optional: Pause game audio if needed
        // AudioListener.pause = true; 
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;           // Unfreezes the game simulation
        isPaused = false;
        pauseMenuUI.SetActive(false);  // Hides the pause menu screen
        GameState.IsPlacing = false;

        // Optional: Resume game audio
        // AudioListener.pause = false; 
    }

    // Optional: Allow the player to also use the 'Escape' key to pause
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
}