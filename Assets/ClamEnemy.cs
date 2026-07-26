using UnityEngine;

public class ClamEnemy : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private string openTrigger = "Open";

    [Header("State")]
    [SerializeField] private bool startsInvulnerable = true;

    private bool isInvulnerable;
    public bool IsInvulnerable => isInvulnerable;

    private bool hasTriggeredThisTry = false;

    private void Awake()
    {
        isInvulnerable = startsInvulnerable;
    }

    private void OnEnable()
    {
        RoundManager.OnTryEnded += HandleTryEnded;
    }

    private void OnDisable()
    {
        RoundManager.OnTryEnded -= HandleTryEnded;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameState.IsPlacing)
            return;

        if (hasTriggeredThisTry)
            return;

        if (!isInvulnerable)
            return;

        FishStats fish = collision.GetComponent<FishStats>();
        if (fish == null)
            return;

        hasTriggeredThisTry = true;

        if (animator != null)
            animator.SetTrigger(closeTrigger);

        TeleportFishOffscreen(fish);

        isInvulnerable = false;
    }
    private void TeleportFishOffscreen(FishStats fish)
    {
        Camera cam = Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(fish.transform.position);

        // Push well past the edge in whichever direction is already closer,
        // so FishOffscreenDisable's padding check catches it immediately next frame
        float pushX = viewportPos.x < 0.5f ? -0.5f : 1.5f;
        Vector3 offscreenViewport = new Vector3(pushX, viewportPos.y, viewportPos.z);

        fish.transform.position = cam.ViewportToWorldPoint(offscreenViewport);
    }

    private void HandleTryEnded()
    {
        if (animator != null)
            animator.SetTrigger(openTrigger);

        isInvulnerable = true;
        hasTriggeredThisTry = false;
    }
}