using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KelpEnemy : MonoBehaviour
{
    [Header("Segment settings")]
    [SerializeField] private GameObject segmentVisualPrefab; // plain sprite-only prefab, NOT the full Kelp fish
    [SerializeField] private float segmentSpacing = 0.5f;
    [SerializeField] private float healthPerSegment = 10f;

    [SerializeField] private HealthManager healthManager;

    [Header("Hit collider (resizes with segment count)")]
    [SerializeField] private BoxCollider2D hitCollider;
    [SerializeField] private float colliderWidth = 0.5f; // tune to match your sprite width

    [Header("Death")]
    [SerializeField] private GameObject destroyRootOverride;

    private int segmentCount;
    public int SegmentCount => segmentCount;

    private List<GameObject> activeSegments = new List<GameObject>();

    public void Init(int segmentCount)
    {
        this.segmentCount = Mathf.Max(1, segmentCount);

        if (hitCollider == null)
            hitCollider = GetComponent<BoxCollider2D>();

        BuildSegmentVisuals();
        ResizeCollider(this.segmentCount);

        float totalHealth = healthPerSegment * this.segmentCount;
        if (healthManager != null)
        {
            healthManager.SetMaxHealth(totalHealth);
        }
    }

    private void BuildSegmentVisuals()
    {
        if (segmentVisualPrefab == null) return;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = Instantiate(segmentVisualPrefab, transform);
            seg.transform.localPosition = new Vector3(0f, i * segmentSpacing, 0f);
            activeSegments.Add(seg);
        }
    }

    public void UpdateSegments(float currentHealth)
    {
        int expectedSegments = Mathf.CeilToInt(currentHealth / healthPerSegment);
        expectedSegments = Mathf.Clamp(expectedSegments, 0, activeSegments.Count);

        while (activeSegments.Count > expectedSegments)
        {
            int topIndex = activeSegments.Count - 1;
            GameObject topSegment = activeSegments[topIndex];
            activeSegments.RemoveAt(topIndex);

            if (topSegment != null)
                Destroy(topSegment);
        }

        ResizeCollider(activeSegments.Count);
    }

    private void ResizeCollider(int currentSegmentCount)
    {
        if (hitCollider == null) return;

        float height = Mathf.Max(segmentSpacing, currentSegmentCount * segmentSpacing);
        hitCollider.size = new Vector2(colliderWidth, height);
        hitCollider.offset = new Vector2(0f, height / 2f); // segments stack upward from y=0
    }

    public void Die()
    {
        if (destroyRootOverride != null)
        {
            Destroy(destroyRootOverride);
        }
        else if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}