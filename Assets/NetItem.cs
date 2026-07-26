using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Net")]
public class NetConsumable : ConsumableItem
{
    [Header("Net settings")]
    [SerializeField] private float pullPercent = 0.5f; // 0.5 = halfway to center
    [SerializeField] private float pullDuration = 0.4f;

    public override void Activate()
    {
        EnemySpawner enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner not found in scene — cannot activate Net.");
            return;
        }

        enemySpawner.PullEnemiesTowardCenter(pullPercent, pullDuration);
        Debug.Log("Net used — pulling enemies toward center.");
    }
}