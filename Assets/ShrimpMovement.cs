using System.Collections;
using UnityEngine;

public class ShrimpMovement : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask fishLayer; // set this to whatever layer your fish are on

    [Header("Thrust")]
    [SerializeField] private float thrustSpeed = 8f;
    [SerializeField] private float decelerationRate = 4f; // higher = stops faster
    [SerializeField] private float minSpeedThreshold = 0.1f; // below this, thrust coroutine ends
    [SerializeField] private float cooldownDuration = 2f;

    private Bounds bounds;
    private bool isThrusting = false;
    private bool isOnCooldown = false;

    public void Init(Bounds spawnBounds)
    {
        bounds = spawnBounds;
    }

    private void Update()
    {
        if (isThrusting || isOnCooldown)
            return;

        Collider2D nearestFish = FindNearestFishInRange();
        if (nearestFish != null)
        {
            Vector2 awayDirection = ((Vector2)transform.position - (Vector2)nearestFish.transform.position).normalized;

            // fallback in case the fish is exactly on top of it
            if (awayDirection == Vector2.zero)
                awayDirection = Random.insideUnitCircle.normalized;

            StartCoroutine(Thrust(awayDirection));
        }
    }

    private Collider2D FindNearestFishInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, fishLayer);

        Collider2D nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            if (hit.transform.IsChildOf(transform)) continue;

            // only actual fish should trigger a thrust
            if (hit.GetComponent<FishStats>() == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hit;
            }
        }

        return nearest;
    }

    private IEnumerator Thrust(Vector2 direction)
    {
        isThrusting = true;

        // Rotate so the sprite's back (default-facing left) points along the thrust direction
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector2 velocity = direction * thrustSpeed;

        while (velocity.magnitude > minSpeedThreshold)
        {
            Vector2 nextPos = (Vector2)transform.position + velocity * Time.deltaTime;

            nextPos.x = Mathf.Clamp(nextPos.x, bounds.min.x, bounds.max.x);
            nextPos.y = Mathf.Clamp(nextPos.y, bounds.min.y, bounds.max.y);

            transform.position = nextPos;

            velocity = Vector2.Lerp(velocity, Vector2.zero, decelerationRate * Time.deltaTime);

            yield return null;
        }

        isThrusting = false;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }
}
