using UnityEngine;

public class WaterStriderMovement : MonoBehaviour
{
    [Header("Orbit settings")]
    [SerializeField] private float radius = 3f;
    private Vector2 center;

    [Header("Push settings")]
    [SerializeField] private float pushStrength = 90f;
    [SerializeField] private float minPushInterval = 1f;
    [SerializeField] private float maxPushInterval = 2.5f;
    [SerializeField] private float deceleration = 60f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private float startMovingThreshold = 10f; // must exceed this to start "moving"
    [SerializeField] private float stopMovingThreshold = 3f;   // must drop below this to stop "moving"

    private float angle;
    private float angularVelocity;
    private float pushTimer;
    private bool isMoving = false; // tracked persistently, not recalculated fresh each frame

    public void Init(Vector2 centerPos, float startRadius)
    {
        center = centerPos;
        radius = startRadius;

        Vector2 offset = (Vector2)transform.position - center;
        angle = Mathf.Atan2(offset.y, offset.x);

        SetNextPushTimer();
    }

    void Update()
    {
        pushTimer -= Time.deltaTime;
        if (pushTimer <= 0f)
        {
            float direction = Random.value < 0.5f ? 1f : -1f;
            angularVelocity += direction * pushStrength;
            SetNextPushTimer();
        }

        angularVelocity = Mathf.MoveTowards(angularVelocity, 0f, deceleration * Time.deltaTime);

        angle += angularVelocity * Mathf.Deg2Rad * Time.deltaTime;

        Vector2 newPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        // analytically compute tangent direction of travel around the circle,
        // instead of diffing positions (which breaks down at low speed)
        if (Mathf.Abs(angularVelocity) > 0.01f)
        {
            float travelSign = Mathf.Sign(angularVelocity);
            Vector2 tangent = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * travelSign;

            float targetAngle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }

        transform.position = newPos;

        // update "is moving" state with hysteresis to prevent rapid flip-flopping near the threshold
        if (animator != null)
        {
            float absVelocity = Mathf.Abs(angularVelocity);

            if (!isMoving && absVelocity > startMovingThreshold)
            {
                isMoving = true;
                animator.SetBool(isMovingParam, true);
            }
            else if (isMoving && absVelocity < stopMovingThreshold)
            {
                isMoving = false;
                animator.SetBool(isMovingParam, false);
            }
        }
    }

    private void SetNextPushTimer()
    {
        pushTimer = Random.Range(minPushInterval, maxPushInterval);
    }
}