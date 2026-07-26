using UnityEngine;

public class SnailEnemy : MonoBehaviour
{
    private enum SnailState { Idle, Hiding, Hidden, Rising }

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hideTrigger = "Hide";
    [SerializeField] private string riseTrigger = "Rise";

    [Header("Animation clip lengths (set to match your actual clips)")]
    [SerializeField] private float hideAnimLength = 0.5f;
    [SerializeField] private float riseAnimLength = 0.5f;

    public bool IsInvulnerable { get; private set; } = false;

    private SnailState state = SnailState.Idle;
    private float timer;
    private bool waitingForTryEnd = false;

    private void OnEnable()
    {
        RoundManager.OnTryEnded += HandleTryEnded;
    }

    private void OnDisable()
    {
        RoundManager.OnTryEnded -= HandleTryEnded;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
        }
        else
        {
            return; // nothing to do — waiting on either an animation timer or the try-end event
        }

        switch (state)
        {
            case SnailState.Hiding:
                EnterHidden();
                break;

            case SnailState.Rising:
                ReturnToIdle();
                break;
        }
    }

    // Called externally (from Health) when the snail takes a hit while vulnerable
    public void RegisterHit()
    {
        if (state != SnailState.Idle) return; // already hiding/hidden — ignore further hits

        StartHiding();
    }

    private void StartHiding()
    {
        state = SnailState.Hiding;
        timer = hideAnimLength;
        IsInvulnerable = true; // safe as soon as it starts ducking in

        if (animator != null)
            animator.SetTrigger(hideTrigger);
    }

    private void EnterHidden()
    {
        state = SnailState.Hidden;
        waitingForTryEnd = true;
        // no timer here — waits for RoundManager.OnTryEnded instead of a fixed duration
    }

    private void HandleTryEnded()
    {
        if (state != SnailState.Hidden || !waitingForTryEnd) return;

        waitingForTryEnd = false;
        StartRising();
    }

    private void StartRising()
    {
        state = SnailState.Rising;
        timer = riseAnimLength;
        // still invulnerable while rising, matching "invulnerable until try ends, then plays unhide" —
        // set to false in ReturnToIdle once fully back to normal instead

        if (animator != null)
            animator.SetTrigger(riseTrigger);
    }

    private void ReturnToIdle()
    {
        state = SnailState.Idle;
        IsInvulnerable = false; // vulnerable again once fully idle
    }
}