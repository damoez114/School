using UnityEngine;

public class LilyEnemy : MonoBehaviour
{
    [Header("Enemies to spawn on death")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int spawnCount = 3;

    [Header("Float out settings")]
    [SerializeField] private float floatDistance = 1.5f;
    [SerializeField] private float floatDuration = 0.6f;

    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        SpawnFloatingEnemies();

        Destroy(gameObject);
    }

    private void SpawnFloatingEnemies()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);

            EnemySpawner.enemyNum++; // keep the global count in sync with actual live enemies

            float angle = (360f / spawnCount) * i + Random.Range(-15f, 15f);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            FloatOutSpawn floatOut = enemy.GetComponent<FloatOutSpawn>();
            if (floatOut != null)
            {
                floatOut.Init(direction, floatDistance, floatDuration);
            }
        }
    }
}