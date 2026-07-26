using UnityEngine;
using System.Collections;

public class CrabMovement : MonoBehaviour, IFreezable
{
    [Header("Movement settings")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float idleDuration = 1.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParam = "IsMoving";

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingRight;
    private bool isPaused = false;

    public void Init(float spawnAreaCenterX)
    {
        startPos = transform.position;
        movingRight = transform.position.x < spawnAreaCenterX;

        float direction = movingRight ? 1f : -1f;
        targetPos = startPos + new Vector3(direction * moveDistance, 0f, 0f);

        StartCoroutine(CrabSequence());
    }

    public void OnFrozen()
    {
        isPaused = true;
        SetMoving(false);
    }

    public void OnUnfrozen()
    {
        isPaused = false;
    }

    private IEnumerator CrabSequence()
    {
        while (true)
        {
            yield return MoveTo(targetPos);

            SetMoving(false);
            yield return WaitWhilePaused(idleDuration);

            yield return MoveTo(startPos);

            SetMoving(false);
            yield return WaitWhilePaused(idleDuration);
        }
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            if (isPaused)
            {
                SetMoving(false);
                yield return null;
                continue;
            }

            SetMoving(true);
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
    }

    // Same as WaitForSeconds, but pauses the countdown while frozen instead of ticking through it
    private IEnumerator WaitWhilePaused(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!isPaused)
                elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void SetMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool(isMovingParam, moving);
        }
    }
}