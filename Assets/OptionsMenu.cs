using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void OnEnable()
    {
        if (volumeSlider != null && SFXManager.Instance != null)
            volumeSlider.SetValueWithoutNotify(SFXManager.Instance.MasterVolume);
    }

    public void OnVolumeSliderChanged(float value)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.SetVolume(value);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(0);
    }
    public void OpenMenu()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    
}

