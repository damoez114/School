using UnityEngine;

public class ButterflyMovement : MonoBehaviour
{
    [Header("Wander settings")]
    [SerializeField] private float speed = 1f; // slow, world units per second
    [SerializeField] private float turnNoiseSpeed = 0.5f; // how quickly the curve direction drifts — lower = long lazy arcs
    [SerializeField] private float maxTurnRate = 90f; // degrees per second — caps how sharp the curve can get

    [Header("Screen bounds")]
    [SerializeField] private float boundsPadding = 1f; // start steering back once this close to the edge
    [SerializeField] private float edgeTurnRateMultiplier = 2f; // turn back toward center faster than normal wandering

    [Header("Rotation")]
    [SerializeField] private bool rotateToFaceDirection = true;
    [SerializeField] private float rotationOffset = 0f; // adjust if the sprite's "forward" isn't pointing right by default

    private float currentAngle; // degrees — current heading, drives both movement AND rotation
    private float noiseSeed;    // offsets each butterfly's noise sample so multiple don't move identically
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    // Called post-Instantiate, same as TurtleMovement.Init()
    public void Init()
    {
        currentAngle = Random.Range(0f, 360f);
        noiseSeed = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        if (IsNearEdge(out Vector2 towardCenterDir))
        {
            // Override the drift and curve firmly back toward center
            float targetAngle = Mathf.Atan2(towardCenterDir.y, towardCenterDir.x) * Mathf.Rad2Deg;
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxTurnRate * edgeTurnRateMultiplier * Time.deltaTime);
        }
        else
        {
            // Continuous, ever-changing curve — Perlin noise never sits still
            // at 0, so the heading is always gently arcing one way or the
            // other instead of holding a straight line between turns.
            float noise = Mathf.PerlinNoise(Time.time * turnNoiseSpeed + noiseSeed, 0f) * 2f - 1f; // -1..1
            currentAngle += noise * maxTurnRate * Time.deltaTime;
        }

        Vector2 moveDirection = AngleToVector(currentAngle);
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Rotation and movement both derive from currentAngle, so the sprite
        // stays tangent to the curve at every point along it.
        if (rotateToFaceDirection)
            transform.rotation = Quaternion.Euler(0, 0, currentAngle + rotationOffset);
    }

    private bool IsNearEdge(out Vector2 towardCenter)
    {
        towardCenter = Vector2.zero;
        if (cam == null) return false;

        float z = -cam.transform.position.z;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, z));

        bool nearEdge =
            transform.position.x < bottomLeft.x + boundsPadding ||
            transform.position.x > topRight.x - boundsPadding ||
            transform.position.y < bottomLeft.y + boundsPadding ||
            transform.position.y > topRight.y - boundsPadding;

        if (nearEdge)
        {
            Vector2 screenCenter = (Vector2)((bottomLeft + topRight) / 2f);
            towardCenter = (screenCenter - (Vector2)transform.position).normalized;
        }

        return nearEdge;
    }

    private Vector2 AngleToVector(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}