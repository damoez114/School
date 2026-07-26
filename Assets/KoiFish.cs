using UnityEngine;

public class KoiFish : MonoBehaviour
{
    [SerializeField] private int hitsPerGrowth = 5;
    [SerializeField] private float growthScalePercent = 0.10f;
    [SerializeField] private float growthDamageAmount = 3f;

    private FishStats fishStats;
    private int hitCount;

    private void Awake()
    {
        fishStats = GetComponent<FishStats>();
    }

    // Called every time this fish successfully hits an enemy
    public void RegisterHit()
    {
        hitCount++;
        Debug.Log(gameObject.name + " Koi hit count: " + hitCount);

        if (hitCount % hitsPerGrowth == 0)
        {
            Debug.Log("GROWING - scale before: " + transform.localScale + " damage before: " + fishStats.damage);
            fishStats.Grow(growthScalePercent);
            fishStats.damage += growthDamageAmount;
            Debug.Log("GREW - scale after: " + transform.localScale + " damage after: " + fishStats.damage);
        }
    }

    public int GetHitCount() => hitCount;
}