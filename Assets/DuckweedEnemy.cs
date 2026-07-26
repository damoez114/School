using UnityEngine;

[RequireComponent(typeof(Health))]
public class DuckweedEnemy : MonoBehaviour
{
    [Header("Float out settings (same feel as Boulder's adds)")]
    [SerializeField] private float floatDistance = 1.2f;
    [SerializeField] private float floatDuration = 0.4f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        RoundManager.OnTryEnded += HandleTryEnded;
    }

    private void OnDisable()
    {
        RoundManager.OnTryEnded -= HandleTryEnded;
    }

    private void HandleTryEnded()
    {
        // Only spawn a clone if this Duckweed survived the try
        if (health == null || health.IsDead)
            return;

        SpawnClone();
    }

    private void SpawnClone()
    {
        Vector2 origin = transform.position;

        float angle = Random.Range(0f, 360f);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        GameObject clone = Instantiate(gameObject, origin, Quaternion.identity);

        EnemySpawner.enemyNum++;

        FloatOutSpawn floatOut = clone.GetComponent<FloatOutSpawn>();
        if (floatOut != null)
        {
            floatOut.Init(direction, floatDistance, floatDuration);
        }
    }
}