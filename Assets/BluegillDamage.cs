using Unity.VisualScripting;
using UnityEngine;

public class FishStats : MonoBehaviour
{
    public bool isGoldfish;
    public bool damageMultOn = false;
    public static float damageMult = 1.0f;
    public float tripleHook = 0f; // <-- was static, now per-fish
    public float damage = 10f;    // <-- removed "* damageMult + tripleHook" from the initializer since both are now applied at hit-time in Health.cs, not baked in at field-init time

    [Header("Growth (Worm consumable)")]
    [SerializeField] private float sizeMultiplier = 1f;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Grow(float percentIncrease)
    {
        sizeMultiplier += percentIncrease;
        transform.localScale = baseScale * sizeMultiplier;
    }

    public void SetGrowthMultiplier(float multiplier)
    {
        sizeMultiplier = multiplier;
        transform.localScale = baseScale * sizeMultiplier;
    }

    public void AddTripleHookBonus(float bonus)
    {
        tripleHook += bonus;
    }
}