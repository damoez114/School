using UnityEngine;

public class HalibutController : MonoBehaviour
{
    [Header("Radius settings")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private LineRenderer radiusLine; // on a child object, add to DraggableFish's extraToggleObjects
    [SerializeField] private Material dottedRadiusMaterial;
    [SerializeField] private int radiusSegments = 64;
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Bonus settings")]
    [SerializeField] private int bonusShells = 1;

    public static bool IsActiveInScene = false;

    private void OnEnable()
    {
        IsActiveInScene = true;
        Health.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        IsActiveInScene = false;
        Health.OnEnemyDied -= HandleEnemyDied;
    }

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
    }

    private void HandleEnemyDied(Vector3 deathPosition)
    {
        if (!SpriteMover.shouldMove) return; // ignore deaths during placement phase, same as Pufferfish

        float effectiveRadius = radius * transform.lossyScale.x;
        float dist = Vector3.Distance(transform.position, deathPosition);

        if (dist <= effectiveRadius)
        {
            ShellManager.Instance.AddShells(bonusShells);
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
        HalibutController[] all = FindObjectsByType<HalibutController>(FindObjectsSortMode.None);
        foreach (var h in all)
        {
            Destroy(h.gameObject);
        }
    }
}
