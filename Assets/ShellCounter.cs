using System.Collections;
using TMPro;
using UnityEngine;

public class ShellManager : MonoBehaviour
{
    public static ShellManager Instance;

    [SerializeField] private TMP_Text shellText;

    [Header("Pop settings")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float popScale = 1.3f;      // how big it pops to
    [SerializeField] private float wobbleAngle = 8f;     // max rotation in degrees

    private int shells = 0;
    private RectTransform shellRect;
    private Vector3 originalScale;
    private Coroutine popRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        shellRect = shellText.GetComponent<RectTransform>();
        originalScale = shellRect.localScale;

        UpdateUI(pop: false); // don't animate on initial load
    }

    public void AddShells(int amount)
    {
        shells += amount;
        UpdateUI();
    }

    public bool SpendShells(int amount)
    {
        if (shells < amount)
            return false;

        shells -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI(bool pop = true)
    {
        shellText.text = "x" + shells.ToString();

        if (pop)
        {
            if (popRoutine != null)
                StopCoroutine(popRoutine);

            popRoutine = StartCoroutine(PopText());
        }
    }

    private IEnumerator PopText()
    {
        float elapsed = 0f;
        float randomWobbleDir = Random.Range(0, 2) == 0 ? -1f : 1f; // random left/right tilt each time

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            // Overshoot-and-settle curve: pops big, slightly overshoots past 1, settles back to normal
            float scaleT = EaseOutBack(t);
            float scale = Mathf.LerpUnclamped(popScale, 1f, scaleT);

            // Rotation wobble decays to 0 as the pop settles
            float wobble = Mathf.Sin(t * Mathf.PI) * wobbleAngle * randomWobbleDir * (1f - t);

            shellRect.localScale = originalScale * scale;
            shellRect.localRotation = Quaternion.Euler(0, 0, wobble);

            yield return null;
        }

        shellRect.localScale = originalScale;
        shellRect.localRotation = Quaternion.identity;
        popRoutine = null;
    }

    // Standard "back ease out" curve — overshoots past the target then settles, very "juicy" feeling
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public int GetShells()
    {
        return shells;
    }
}