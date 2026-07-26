using UnityEngine;

public class FrogEnemy : MonoBehaviour
{
    private enum FrogState { Idle, Hiding, Hidden, Rising }

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hideTrigger = "Hide";
    [SerializeField] private string riseTrigger = "Rise";

    [Header("Timing")]
    [SerializeField] private float minIdleDuration = 3f;
    [SerializeField] private float maxIdleDuration = 6f;
    [SerializeField] private float hiddenDuration = 2.5f; // how long it stays invulnerable once fully hidden

    [Header("Animation clip lengths (set to match your actual clips)")]
    [SerializeField] private float hideAnimLength = 0.5f;
    [SerializeField] private float riseAnimLength = 0.5f;

    public bool IsInvulnerable { get; private set; } = false;

    private FrogState state = FrogState.Idle;
    private float timer;

    void Start()
    {
        SetNextIdleTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        switch (state)
        {
            case FrogState.Idle:
                StartHiding();
                break;

            case FrogState.Hiding:
                EnterHidden();
                break;

            case FrogState.Hidden:
                StartRising();
                break;

            case FrogState.Rising:
                ReturnToIdle();
                break;
        }
    }

    private void StartHiding()
    {
        state = FrogState.Hiding;
        timer = hideAnimLength;

        if (animator != null)
            animator.SetTrigger(hideTrigger);
    }

    private void EnterHidden()
    {
        state = FrogState.Hidden;
        timer = hiddenDuration;
        IsInvulnerable = true; // frog can't take damage while fully hidden
    }

    private void StartRising()
    {
        state = FrogState.Rising;
        timer = riseAnimLength;
        IsInvulnerable = false; // vulnerable again as soon as it starts rising

        if (animator != null)
            animator.SetTrigger(riseTrigger);
    }

    private void ReturnToIdle()
    {
        state = FrogState.Idle;
        SetNextIdleTimer();
    }

    private void SetNextIdleTimer()
    {
        timer = Random.Range(minIdleDuration, maxIdleDuration);
    }
}