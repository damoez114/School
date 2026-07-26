using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Action = System.Action; // <-- instead of "using System;"

public class RoundManager : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public HotbarManager hotbarManager;
    public GameObject pauseButton;
    public GameObject winScreen;
    public GameObject loseScreen;
    public ShopManager shopManager;
    public static bool isReloading = false;
    [SerializeField] private TMP_Text triesText;
    public static int initialtries = 3;
    private int tries = initialtries;
    private bool isPlaying = true;
    private int roundCounter = 1;

    [Header("Tries pop settings")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float wobbleAngle = 8f;

    private RectTransform triesRect;
    private Vector3 originalScale;
    private Coroutine popRoutine;
    public int RoundCounter => roundCounter;
    public void Start()
    {
        hotbarManager.SaveHotbar();
        isReloading = false;

        triesRect = triesText.GetComponent<RectTransform>();
        originalScale = triesRect.localScale;
    }

    public void Update()
    {
        CheckWin();
    }

    public static event Action OnTryEnded;

    public void CheckWin()
    {
        if (isReloading)
            return;
        if (EnemySpawner.enemyNum == 0 && Roe.fishCount == 0 && isPlaying == true)
        {
            if (ShellManager.Instance != null && tries > 0)
                ShellManager.Instance.AddShells(tries);

            tries = initialtries;
            UpdateUI();
            PufferfishController.DespawnActive();
            HalibutController.DespawnActive();
            ElectricEelController.DespawnActive();
            hotbarManager.ClearHotbar();
            hotbarManager.LoadHotbar();
            winScreen.SetActive(true);
            isPlaying = false;
            hotbarManager.resetCount();
            HotbarManager.roeUsedThisAttempt = false;

            // Round is fully resolved — wipe the replay memory.
            if (RoundPlacementManager.Instance != null)
                RoundPlacementManager.Instance.ClearRoundMemory();

            OnTryEnded?.Invoke();
        }
        else if (EnemySpawner.enemyNum > 0 && Roe.fishCount == 0 && GameState.IsPlacing == true)
        {
            GameState.IsPlacing = false;
            pauseButton.SetActive(false);
            hotbarManager.ClearHotbar();
            hotbarManager.LoadHotbar(restoreUniqueItems: false); // <-- pufferfish stays out until you win
            triesReset();
            hotbarManager.resetCount();
            HotbarManager.roeUsedThisAttempt = false;
            DraggableFish.SetMovementStarted(false); // <-- next attempt — fish are draggable again

            OnTryEnded?.Invoke();
        }
        else if (EnemySpawner.enemyNum > 0 && Roe.fishCount == 0 && tries == 0)
        {
            // Game over — wipe the replay memory.
            if (RoundPlacementManager.Instance != null)
                RoundPlacementManager.Instance.ClearRoundMemory();

            loseScreen.SetActive(true);
        }
    }

    private bool isResetting = false;

    [Header("Sound")]
    [SerializeField] private AudioClip nextRoundSound;

    public void RoundReset()
    {
        if (isResetting) return;
        isResetting = true;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(nextRoundSound);

        GameState.IsPlacing = false;
        SpriteMover.shouldMove = false;
        pauseButton.SetActive(false);
        tries = initialtries;
        isPlaying = true;
        roundCounter++;
        enemySpawner.SpawnEnemies();
        DraggableFish.SetMovementStarted(false); // <-- next round's placement phase — fish are draggable again

        shopManager.CloseShop();
        shopManager.RefreshShop();
        shopManager.AdvanceRound();

        isResetting = false;
    }

    public void triesReset()
    {
        SpriteMover.shouldMove = false;
        pauseButton.SetActive(false);
    }

    private void UpdateUI()
    {
        triesText.text = "x" + tries.ToString();

        if (popRoutine != null)
            StopCoroutine(popRoutine);

        popRoutine = StartCoroutine(PopText());
    }

    private IEnumerator PopText()
    {
        float elapsed = 0f;
        float randomWobbleDir = Random.Range(0, 2) == 0 ? -1f : 1f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float scaleT = EaseOutBack(t);
            float scale = Mathf.LerpUnclamped(popScale, 1f, scaleT);

            float wobble = Mathf.Sin(t * Mathf.PI) * wobbleAngle * randomWobbleDir * (1f - t);

            triesRect.localScale = originalScale * scale;
            triesRect.localRotation = Quaternion.Euler(0, 0, wobble);

            yield return null;
        }

        triesRect.localScale = originalScale;
        triesRect.localRotation = Quaternion.identity;
        popRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public static void IncreaseInitialTries(int amount)
    {
        initialtries += amount;
    }

    public void GrantExtraTryNow(int amount)
    {
        initialtries += amount;
        tries += amount;
        UpdateUI();
    }

    public void LowerRounds(int amount)
    {
        // Already at or near the floor — just snap to the minimum difficulty
        if (roundCounter <= 2)
        {
            roundCounter = 1;
            return;
        }

        roundCounter -= amount;

        // Difficulty 6 is an extra-harsh spike — if we land exactly on it,
        // knock off one more so it doesn't feel like a dead stop.
        if (roundCounter == 6)
            roundCounter -= 1;

        if (roundCounter < 1)
            roundCounter = 1;
    }

    public void GrantTemporaryTry(int amount)
    {
        tries += amount;
        UpdateUI();
    }

    // Called from GameManager.StartFish — tries now go down the moment you
    // commit to an attempt, not after it resolves.
    public void UseTry()
    {
        tries = Mathf.Max(0, tries - 1);
        UpdateUI();
    }
}