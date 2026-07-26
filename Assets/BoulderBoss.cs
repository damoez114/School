using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderBoss : MonoBehaviour
{
    [Serializable]
    public class AddEntry
    {
        public GameObject prefab;
        [Tooltip("Relative spawn weight — higher means more likely to be picked")]
        public float spawnWeight = 1f;
    }

    [Header("Add Spawning")]
    [SerializeField] private AddEntry[] possibleAdds;
    [SerializeField] private int minAddsPerHit = 1;
    [SerializeField] private int maxAddsPerHit = 2;

    [Header("Float out settings (matches LilyEnemy's spore motion)")]
    [SerializeField] private float floatDistance = 1.5f;
    [SerializeField] private float floatDuration = 0.4f;

    [Header("Visuals while floating out")]
    [SerializeField] private bool disableCollisionWhileFloating = true;
    [SerializeField] private int floatingSortingOrder = 100; // high enough to beat everything else on screen

    private Bounds spawnBounds;
    private bool boundsSet = false;

    // Called once by EnemySpawner right after Instantiate, same pattern as
    // CrabMovement.Init / DragonflyEnemy.Init
    public void Init(Bounds bounds)
    {
        spawnBounds = bounds;
        boundsSet = true;
    }

    // Called from Health.OnTriggerStay2D every time this boss takes a non-lethal hit
    public void OnDamaged()
    {
        int spawnCount = UnityEngine.Random.Range(minAddsPerHit, maxAddsPerHit + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOneAdd(i, spawnCount);
        }
    }

    private void SpawnOneAdd(int index, int totalCount)
    {
        GameObject prefab = PickWeightedAdd();
        if (prefab == null) return;

        Vector2 origin = transform.position;

        // Randomize the base angle so a single add (totalCount == 1) doesn't always
        // land in the same spot — only the spread between multiple simultaneous
        // adds should be even, not their overall starting direction
        float baseAngle = UnityEngine.Random.Range(0f, 360f);
        float angle = baseAngle + (360f / totalCount) * index + UnityEngine.Random.Range(-15f, 15f);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        GameObject add = Instantiate(prefab, origin, Quaternion.identity);

        // Same Init hookups EnemySpawner does, so adds behave like normally spawned enemies
        WaterStriderMovement strider = add.GetComponent<WaterStriderMovement>();
        if (strider != null)
            strider.Init(origin, 1f);

        CrabMovement crab = add.GetComponent<CrabMovement>();
        if (crab != null)
            crab.Init(origin.x + direction.x * floatDistance);

        DragonflyEnemy dragonfly = add.GetComponent<DragonflyEnemy>();
        if (dragonfly != null && boundsSet)
            dragonfly.Init(spawnBounds);

        EnemySpawner.enemyNum++;

        FloatOutSpawn floatOut = add.GetComponent<FloatOutSpawn>();
        if (floatOut != null)
        {
            floatOut.Init(direction, floatDistance, floatDuration);
        }

        // Collision disable + render-on-top are boss-specific extras Lily's spores
        // don't need — run them in parallel on the add's own Health component so
        // they survive independently of the boss (same reasoning as before: never
        // host a coroutine on the boss itself, since it can be destroyed mid-flight)
        Health addHealth = add.GetComponent<Health>();
        if (addHealth != null)
            addHealth.StartCoroutine(ApplySpawnVisuals(add, floatDuration));
    }

    private IEnumerator ApplySpawnVisuals(GameObject add, float duration)
    {
        if (add == null) yield break;

        Collider2D col = add.GetComponent<Collider2D>();
        SpriteRenderer sr = add.GetComponentInChildren<SpriteRenderer>();
        Behaviour[] behaviours = add.GetComponents<Behaviour>();

        int originalSortingOrder = 0;
        if (sr != null)
        {
            originalSortingOrder = sr.sortingOrder;
            sr.sortingOrder = floatingSortingOrder;
        }

        if (disableCollisionWhileFloating && col != null)
            col.enabled = false;

        // Disable everything except Health and FloatOutSpawn itself, so movement
        // scripts (CrabMovement, WaterStriderMovement, etc.) don't fight the float
        List<Behaviour> disabled = new List<Behaviour>();
        foreach (Behaviour b in behaviours)
        {
            if (b is Health) continue;
            if (b is FloatOutSpawn) continue;
            if (b.enabled)
            {
                b.enabled = false;
                disabled.Add(b);
            }
        }

        yield return new WaitForSeconds(duration);

        if (add == null) yield break;

        foreach (Behaviour b in disabled)
        {
            if (b != null) b.enabled = true;
        }

        if (disableCollisionWhileFloating && col != null)
            col.enabled = true;

        if (sr != null)
            sr.sortingOrder = originalSortingOrder;
    }

    private GameObject PickWeightedAdd()
    {
        float totalWeight = 0f;
        foreach (var entry in possibleAdds)
        {
            if (entry.prefab != null)
                totalWeight += Mathf.Max(entry.spawnWeight, 0f);
        }

        if (totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in possibleAdds)
        {
            if (entry.prefab == null) continue;

            cumulative += Mathf.Max(entry.spawnWeight, 0f);
            if (roll <= cumulative)
                return entry.prefab;
        }

        return possibleAdds[possibleAdds.Length - 1].prefab;
    }
}