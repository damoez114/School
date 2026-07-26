using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMover : MonoBehaviour
{
    public enum FishType { Other, Trout, Catfish, Pike, Salmon, Sardine }

    [Header("Pike settings (Pike only)")]
    [SerializeField] private float pikeTargetRadius = 0.3f; // how close counts as "reached" the enemy
    [SerializeField] private float pikeRotationSpeed = 10f;

    private Health currentPikeTarget;
    private HashSet<Health> pikeTargetedEnemies = new HashSet<Health>();
    private bool pikeGoingStraight = false;
    private Vector3 pikeStraightDir;
    [Header("Spiral settings (Salmon only)")]
    [SerializeField] private float spiralStartRadius = 0.3f; // radius at the very start of the spiral
    [SerializeField] private float spiralGrowthRate = 0.15f;  // how much radius grows per radian turned
    [SerializeField] private float salmonRotationSpeed = 10f;
    public float SpiralStartRadius => spiralStartRadius;
    public float SpiralGrowthRate => spiralGrowthRate;

    private float spiralTheta;
    [SerializeField] private FishType fishType = FishType.Other;

    [SerializeField] private float speed = 5f;

    [Header("Wave settings (Trout only)")]
    [SerializeField] private float waveAmplitude = 0.5f;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("U-Shape settings (Catfish only)")]
    [SerializeField] private float uLegLength = 3f;   // length of each straight side of the U
    [SerializeField] private float uRadius = 1.5f;    // radius of the curved bottom of the U
    [SerializeField] private float catfishRotationSpeed = 10f;

    [Header("Sardine settings (Sardine only)")]
    [SerializeField] private SpriteMover leader;       // the lead sardine this one copies
    [SerializeField] private float followDelay = 0.15f; // seconds behind the leader
    private bool leaderLost = false;

    [System.Serializable]
    private struct PoseSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
    }

    private List<PoseSnapshot> poseHistory = new List<PoseSnapshot>();
    private const float historyDuration = 2f; // only needs to exceed max followDelay in use
    public float ULegLength => uLegLength;
    public float URadius => uRadius;

    public static bool shouldMove = false;

    [SerializeField] private GameObject[] spritesToHide;
    [SerializeField] private GameObject arrowsParent;

    private bool hidden = false;
    private bool wasMoving = false;

    private Vector3 startPos;
    private Vector3 lastPos;
    private Vector3 fixedForward;
    private Vector3 fixedRight;
    private float waveTimer = 0f;
    private float distanceTraveled = 0f;

    // Stingray (or any other fish) can briefly freeze movement mid-pattern without resetting its state
    private bool isPaused = false;

    void Update()
    {
        if (!shouldMove)
        {
            wasMoving = false;
            return;
        }

        if (!hidden)
        {
            foreach (GameObject obj in spritesToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
            hidden = true;
        }

        if (!wasMoving)
        {
            startPos = transform.position;
            lastPos = transform.position;
            fixedForward = transform.up;
            fixedRight = transform.right;
            waveTimer = 0f;
            distanceTraveled = 0f;
            spiralTheta = 0.01f; // small nonzero start to avoid a zero-radius singularity
            wasMoving = true;
        }

        if (isPaused)
            return; // hold position (and freeze timers) this frame; wasMoving stays true

        switch (fishType)
        {
            case FishType.Trout:
                MoveInWave();
                break;
            case FishType.Catfish:
                MoveInU();
                break;
            case FishType.Pike:
                MovePike();
                break;
            case FishType.Salmon:
                MoveInSpiral();
                break;
            case FishType.Sardine:
                MoveAsSardineFollower();
                break;
            default:
                transform.position += transform.up * speed * Time.deltaTime;
                break;
        }

        RecordPoseHistory();
    }

    private void RecordPoseHistory()
    {
        poseHistory.Add(new PoseSnapshot { position = transform.position, rotation = transform.rotation, time = Time.time });
        while (poseHistory.Count > 0 && Time.time - poseHistory[0].time > historyDuration)
            poseHistory.RemoveAt(0);
    }
    // Call to freeze this fish's movement in place for a duration (used by Stingray's hit combo)
    public void PauseMovement(float duration)
    {
        if (!isPaused)
            StartCoroutine(PauseRoutine(duration));
    }

    private IEnumerator PauseRoutine(float duration)
    {
        isPaused = true;
        yield return new WaitForSeconds(duration);
        isPaused = false;
    }

    private void MoveInWave()
    {
        waveTimer += Time.deltaTime;

        startPos += fixedForward * speed * Time.deltaTime;

        float offset = Mathf.Sin(waveTimer * waveFrequency) * waveAmplitude;

        Vector3 newPos = startPos + fixedRight * offset;

        Vector3 moveDir = (newPos - lastPos);
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    private void MoveInU()
    {
        distanceTraveled += speed * Time.deltaTime;

        float s = distanceTraveled;
        float arcLength = Mathf.PI * uRadius;

        Vector3 newPos;

        if (s < uLegLength)
        {
            newPos = startPos + fixedForward * s;
        }
        else if (s < uLegLength + arcLength)
        {
            float arcProgress = (s - uLegLength) / arcLength;
            float theta = arcProgress * Mathf.PI;

            Vector3 arcCenter = startPos + fixedForward * uLegLength + fixedRight * uRadius;

            // FIXED: was "- fixedForward * sin(theta)", now bulges forward/outward correctly
            Vector3 offsetDir = -fixedRight * Mathf.Cos(theta) + fixedForward * Mathf.Sin(theta);
            newPos = arcCenter + offsetDir * uRadius;
        }
        else
        {
            float legProgress = s - uLegLength - arcLength;
            Vector3 secondLegStart = startPos + fixedForward * uLegLength + fixedRight * (2f * uRadius);
            newPos = secondLegStart - fixedForward * legProgress;
        }

        Vector3 moveDir = (newPos - lastPos);
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    public static void StartAllFish()
    {
        shouldMove = true;
    }

    public static void StopAllFish()
    {
        shouldMove = false;
    }

    private void MovePike()
    {
        if (pikeGoingStraight)
        {
            transform.position += pikeStraightDir * speed * Time.deltaTime;
            return;
        }

        if (currentPikeTarget == null || currentPikeTarget.IsDead)
        {
            currentPikeTarget = FindNearestUntargetedEnemy();

            if (currentPikeTarget == null)
            {
                pikeGoingStraight = true;
                pikeStraightDir = transform.up;
                return;
            }
        }

        Vector3 toTarget = currentPikeTarget.transform.position - transform.position;
        float distance = toTarget.magnitude;

        // Check "reached" BEFORE moving, so we don't silently overshoot past a close target
        if (distance <= pikeTargetRadius)
        {
            pikeTargetedEnemies.Add(currentPikeTarget);
            currentPikeTarget = null;
            return; // next frame picks a new target (or locks into straight-line mode)
        }

        Vector3 moveDir = toTarget.normalized;

        // Move directly toward the target — guarantees interception regardless of turn speed
        transform.position += moveDir * speed * Time.deltaTime;

        // Rotate the sprite to visually face the direction it's actually moving
        float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private Health FindNearestUntargetedEnemy()
    {
        Health[] allEnemies = FindObjectsByType<Health>(FindObjectsSortMode.None);

        Health nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Health enemy in allEnemies)
        {
            if (enemy.IsDead) continue;
            if (pikeTargetedEnemies.Contains(enemy)) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
    private void MoveInSpiral()
    {
        float r = spiralStartRadius + spiralGrowthRate * spiralTheta;

        float dtheta = (speed * Time.deltaTime) / Mathf.Max(r, 0.05f);

        // Clamp so the fish never spins faster than this many radians per frame,
        // even when the radius is tiny at the start of the spiral
        float maxDeltaTheta = 3f * Time.deltaTime; // ~3 rad/sec max spin rate — tune this
        dtheta = Mathf.Min(dtheta, maxDeltaTheta);

        spiralTheta += dtheta;

        r = spiralStartRadius + spiralGrowthRate * spiralTheta;

        Vector3 offsetDir = fixedRight * Mathf.Cos(spiralTheta) + fixedForward * Mathf.Sin(spiralTheta);
        Vector3 newPos = startPos + offsetDir * r;

        Vector3 moveDir = newPos - lastPos;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position = newPos;
        lastPos = newPos;
    }
    private void MoveAsSardineFollower()
    {
        if (leaderLost)
        {
            // Leader is gone — keep swimming in the direction we were last facing
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        if (leader == null)
        {
            leaderLost = true;
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        float targetTime = Time.time - followDelay;
        List<PoseSnapshot> leaderHistory = leader.poseHistory;

        if (leaderHistory.Count == 0)
        {
            // Leader has no history left (just got wiped) — bail into fallback too
            leaderLost = true;
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        for (int i = leaderHistory.Count - 1; i > 0; i--)
        {
            if (leaderHistory[i - 1].time <= targetTime)
            {
                float t = Mathf.InverseLerp(leaderHistory[i - 1].time, leaderHistory[i].time, targetTime);
                transform.position = Vector3.Lerp(leaderHistory[i - 1].position, leaderHistory[i].position, t);
                transform.rotation = Quaternion.Slerp(leaderHistory[i - 1].rotation, leaderHistory[i].rotation, t);
                return;
            }
        }

        if (leaderHistory.Count > 0)
        {
            transform.position = leaderHistory[0].position;
            transform.rotation = leaderHistory[0].rotation;
        }
    }
}