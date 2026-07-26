using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float masterVolume = 1f;
    public float MasterVolume => masterVolume;

    private const string VolumePrefKey = "SFX_VOLUME";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        masterVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume * masterVolume);
    }

    public void SetVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumePrefKey, masterVolume);
        PlayerPrefs.Save();
    }
}