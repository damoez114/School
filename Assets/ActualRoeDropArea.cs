using UnityEngine;

public class ActualRoeDropArea : MonoBehaviour
{
    public void SpawnFish(GameObject fishPrefab, Vector3 position)
    {
        if (fishPrefab == null) return;

        Instantiate(fishPrefab, position, Quaternion.identity);
        Destroy(gameObject);
    }
}
