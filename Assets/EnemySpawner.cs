using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        [Tooltip("Relative spawn weight — higher means more likely to be picked")]
        public float spawnWeight = 1f;
    }

    [Serializable]
    public class DifficultyTier
    {
        [Tooltip("How many enemies spawn at this difficulty (randomly picked in this range)")]
        public int minEnemyCount = 2;
        public int maxEnemyCount = 4;

        public EnemyEntry[] enemies;
    }

    [Header("Difficulty Tiers (index 0 = Difficulty 1, ... index 9 = Difficulty 10)")]
    [SerializeField] private DifficultyTier[] difficultyTiers = new DifficultyTier[10];

    [Header("Rounds spent per difficulty (index 0 = Difficulty 1, ... index 9 = Difficulty 10)")]
    [SerializeField] private int[] roundsPerDifficulty = new int[10] { 2, 2, 2, 2, 2, 1, 2, 2, 2, 2 };

    [Header("Round Manager Reference")]
    [SerializeField] private RoundManager roundManager;

    [Header("Water Strider orbit settings")]
    [SerializeField] private float striderRadius = 2f;

    [Header("Kelp segment settings")]
    [SerializeField] private int kelpMinSegments = 2;
    [SerializeField] private int kelpMaxSegments = 5;

    private BoxCollider2D spawnArea;
    public static int enemyNum = 0;
    private int enemyCount;

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        SpawnEnemies();
    }

    public void PullEnemiesTowardCenter(float pullPercent, float duration)
    {
        Bounds bounds = spawnArea.bounds;
        Vector2 center = bounds.center;

        Health[] allEnemies = FindObjectsByType<Health>(FindObjectsSortMode.None);

        foreach (Health enemy in allEnemies)
        {
            if (enemy.IsDead) continue;
            if (enemy.GetComponent<WaterStriderMovement>() != null) continue;

            StartCoroutine(PullSingleEnemy(enemy.transform, center, pullPercent, duration));
        }
    }

    private IEnumerator PullSingleEnemy(Transform enemyTransform, Vector2 center, float pullPercent, float duration)
    {
        Vector3 startPos = enemyTransform.position;
        Vector3 targetPos = Vector3.Lerp(startPos, center, pullPercent);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (enemyTransform == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            enemyTransform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        if (enemyTransform != null)
            enemyTransform.position = targetPos;
    }

    [Header("Debug")]
    [SerializeField] private bool overrideDifficulty = false;
    [SerializeField] private int debugDifficulty = 1;

    private int GetCurrentDifficulty()
    {
        if (roundManager == null)
            return overrideDifficulty ? Mathf.Clamp(debugDifficulty, 1, 10) : 1;

        int round = roundManager.RoundCounter;
        int startDifficulty = overrideDifficulty ? Mathf.Clamp(debugDifficulty, 1, 10) : 1;

        int cumulativeRounds = 0;

        for (int i = startDifficulty - 1; i < roundsPerDifficulty.Length; i++)
        {
            int roundsForThisTier = Mathf.Max(roundsPerDifficulty[i], 1);
            cumulativeRounds += roundsForThisTier;

            if (round <= cumulativeRounds)
            {
                Debug.Log($"Round {round}, start {startDifficulty} -> difficulty {i + 1} (tier allows {roundsForThisTier} rounds, cumulative {cumulativeRounds})");
                return i + 1;
            }
        }

        return roundsPerDifficulty.Length;
    }

    public void SpawnEnemies()
    {
        int difficulty = GetCurrentDifficulty();
        DifficultyTier tier = GetTier(difficulty);

        if (tier == null || tier.enemies == null || tier.enemies.Length == 0)
        {
            Debug.LogWarning("No enemies configured for difficulty " + difficulty);
            return;
        }

        enemyCount = UnityEngine.Random.Range(tier.minEnemyCount, tier.maxEnemyCount + 1);

        Bounds bounds = spawnArea.bounds;
        Vector2 center = bounds.center;

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefab = PickWeightedEnemy(tier.enemies);
            if (prefab == null) continue;

            WaterStriderMovement striderCheck = prefab.GetComponent<WaterStriderMovement>();
            CrabMovement crabCheck = prefab.GetComponent<CrabMovement>();
            BoulderBoss bossCheck = prefab.GetComponent<BoulderBoss>();
            TurtleMovement turtleCheck = prefab.GetComponent<TurtleMovement>();
            KelpEnemy kelpCheck = prefab.GetComponent<KelpEnemy>();

            Vector2 spawnPos;

            if (bossCheck != null)
            {
                spawnPos = center;
            }
            else if (striderCheck != null)
            {
                float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                spawnPos = center + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * striderRadius;
            }
            else if (turtleCheck != null)
            {
                spawnPos = center; // Init will place it correctly on the ellipse right after
            }
            else
            {
                spawnPos = new Vector2(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
                );
            }

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            WaterStriderMovement strider = enemy.GetComponent<WaterStriderMovement>();
            if (strider != null)
            {
                strider.Init(center, striderRadius);
            }

            CrabMovement crab = enemy.GetComponent<CrabMovement>();
            if (crab != null)
            {
                crab.Init(center.x);
            }

            DragonflyEnemy dragonfly = enemy.GetComponent<DragonflyEnemy>();
            if (dragonfly != null)
            {
                dragonfly.Init(bounds);
            }

            BoulderBoss boss = enemy.GetComponent<BoulderBoss>();
            if (boss != null)
            {
                boss.Init(bounds);
            }

            TurtleMovement turtle = enemy.GetComponent<TurtleMovement>();
            if (turtle != null)
            {
                turtle.Init(center);
            }

            KelpEnemy kelp = enemy.GetComponent<KelpEnemy>();
            if (kelp != null)
            {
                int randomSegments = UnityEngine.Random.Range(kelpMinSegments, kelpMaxSegments + 1);
                kelp.Init(randomSegments);
            }

            ShrimpMovement shrimp = enemy.GetComponent<ShrimpMovement>();
            if (shrimp != null)
            {
                shrimp.Init(bounds);
            }
            enemyNum++;
        }

        Debug.Log("Spawned " + enemyCount + " enemies at difficulty " + difficulty + " (Total: " + enemyNum + ")");
    }

    private DifficultyTier GetTier(int difficulty)
    {
        int index = difficulty - 1;
        if (index < 0 || index >= difficultyTiers.Length) return null;
        return difficultyTiers[index];
    }

    private GameObject PickWeightedEnemy(EnemyEntry[] entries)
    {
        float totalWeight = 0f;
        foreach (var entry in entries)
        {
            if (entry.prefab != null)
                totalWeight += Mathf.Max(entry.spawnWeight, 0f);
        }

        if (totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            if (entry.prefab == null) continue;

            cumulative += Mathf.Max(entry.spawnWeight, 0f);
            if (roll <= cumulative)
                return entry.prefab;
        }

        return entries[entries.Length - 1].prefab; // fallback, shouldn't normally hit this
    }
}