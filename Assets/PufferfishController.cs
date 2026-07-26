using System.Collections.Generic;
using UnityEngine;

public class PufferfishController : MonoBehaviour
{
    [Header("Radius settings")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private LineRenderer radiusLine; // on a child object, referenced in DraggableFish's extraToggleObjects
    [SerializeField] private Material dottedRadiusMaterial;
    [SerializeField] private int radiusSegments = 64;
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Damage settings")]
    [SerializeField] private float damagePerPulse = 5f;
    [SerializeField] private DraggableFish draggableFish;
    private FishStats fishStats;

    [Header("Ability trigger (on a separate child object, NOT the root)")]
    [SerializeField] private CircleCollider2D triggerCollider;

    public static bool IsActiveInScene = false;

    private void OnEnable() => IsActiveInScene = true;
    private void OnDisable() => IsActiveInScene = false;

    void Start()
    {
        if (radiusLine != null)
        {
            radiusLine.material = dottedRadiusMaterial;
            radiusLine.loop = true;
            radiusLine.useWorldSpace = false;
            radiusLine.positionCount = radiusSegments;
            radiusLine.startWidth = lineWidth;
            radiusLine.endWidth = lineWidth;
            DrawRadiusCircle();
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            triggerCollider.radius = radius;
        }

        draggableFish = GetComponent<DraggableFish>();
        fishStats = GetComponent<FishStats>();
    }

    private HashSet<Collider2D> collidersInside = new HashSet<Collider2D>();

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!SpriteMover.shouldMove) return;
        if (draggableFish != null && draggableFish.IsDragging) return;
        if (collidersInside.Contains(collision)) return;

        FishStats fish = collision.GetComponentInParent<FishStats>();
        if (fish == null) return;

        collidersInside.Add(collision);
        PulseDamageAllInRadius();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collidersInside.Remove(collision);
    }

    private float EffectiveRadius => triggerCollider.radius * transform.lossyScale.x;

    private void PulseDamageAllInRadius()
    {
        float effectiveRadius = EffectiveRadius;
        float totalDamage = damagePerPulse + (fishStats != null ? fishStats.tripleHook : 0f);

        Health[] allEnemies = FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (Health enemy in allEnemies)
        {
            if (enemy.IsDead) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= effectiveRadius)
            {
                enemy.ApplyDamage(totalDamage, transform.position);
            }
        }
    }

    private void DrawRadiusCircle()
    {
        for (int i = 0; i < radiusSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / radiusSegments;
            radiusLine.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    public static void DespawnActive()
    {
        PufferfishController[] all = FindObjectsByType<PufferfishController>(FindObjectsSortMode.None);
        foreach (var p in all)
        {
            Destroy(p.gameObject);
        }
    }

}