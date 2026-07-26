using System.Collections.Generic;
using UnityEngine;

public class ElectricEelController : MonoBehaviour, IRadiusTriggerListener
{
    [Header("Radius settings")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private LineRenderer radiusLine; // on a child object, add to DraggableFish's extraToggleObjects
    [SerializeField] private Material dottedRadiusMaterial;
    [SerializeField] private int radiusSegments = 64;
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Ability trigger (on a separate child object, NOT the root)")]
    [SerializeField] private CircleCollider2D triggerCollider;

    [SerializeField] private DraggableFish draggableFish;

    public static bool IsActiveInScene = false;

    private Dictionary<GameObject, List<Behaviour>> frozenEnemies = new Dictionary<GameObject, List<Behaviour>>();
    private Dictionary<GameObject, List<IFreezable>> frozenFreezables = new Dictionary<GameObject, List<IFreezable>>();
    private bool wasDragging = false;

    private void OnEnable() => IsActiveInScene = true;

    private void OnDisable()
    {
        IsActiveInScene = false;

        if (!isReloadingScene)
            UnfreezeAll();
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

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            triggerCollider.radius = radius;
        }

        draggableFish = GetComponent<DraggableFish>();
    }

    void Update()
    {
        if (draggableFish == null) return;

        if (draggableFish.IsDragging && !wasDragging)
        {
            // Just started being dragged — release everyone caught so far
            UnfreezeAll();
        }
        else if (!draggableFish.IsDragging && wasDragging)
        {
            // Just got dropped — re-freeze anyone still sitting in the radius
            RefreezeOverlapping();
        }

        wasDragging = draggableFish.IsDragging;
    }

    public void OnRadiusTriggerEnter2D(Collider2D collision) { } // no longer used

    public void OnRadiusTriggerStay2D(Collider2D collision)
    {
        if (draggableFish != null && draggableFish.IsDragging) return;

        Health enemy = collision.GetComponentInParent<Health>();
        if (enemy == null || enemy.IsDead) return;

        FreezeEnemy(enemy.gameObject);
    }

    public void OnRadiusTriggerExit2D(Collider2D collision)
    {
        Health enemy = collision.GetComponentInParent<Health>();
        if (enemy == null) return;

        UnfreezeEnemy(enemy.gameObject);
    }


    private void FreezeEnemy(GameObject enemyObj)
    {
        if (frozenEnemies.ContainsKey(enemyObj)) return;

        List<Behaviour> disabledScripts = new List<Behaviour>();
        List<IFreezable> pausedScripts = new List<IFreezable>();
        Behaviour[] behaviours = enemyObj.GetComponents<Behaviour>();

        foreach (var b in behaviours)
        {
            if (b == null || b is Health) continue;
            if (b is Collider2D) continue;
            if (b is Renderer) continue;
            if (!b.enabled) continue;

            if (b is IFreezable freezable)
            {
                freezable.OnFrozen();
                pausedScripts.Add(freezable);
                continue; // leave it enabled — its own pause flag handles the freeze, coroutine stays alive
            }

            if (b is MonoBehaviour mb)
                mb.StopAllCoroutines();

            b.enabled = false;
            disabledScripts.Add(b);
        }

        frozenEnemies[enemyObj] = disabledScripts;
        frozenFreezables[enemyObj] = pausedScripts;

        Rigidbody2D rb = enemyObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }

    private void UnfreezeEnemy(GameObject enemyObj)
    {
        if (frozenEnemies.TryGetValue(enemyObj, out List<Behaviour> disabledScripts))
        {
            foreach (var b in disabledScripts)
                if (b != null) b.enabled = true;

            frozenEnemies.Remove(enemyObj);
        }

        if (frozenFreezables.TryGetValue(enemyObj, out List<IFreezable> pausedScripts))
        {
            foreach (var f in pausedScripts)
                f?.OnUnfrozen();

            frozenFreezables.Remove(enemyObj);
        }

        Rigidbody2D rb = enemyObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.sleepMode = RigidbodySleepMode2D.StartAwake;
        }
    }

    private void UnfreezeAll()
    {
        List<GameObject> keys = new List<GameObject>(frozenEnemies.Keys);
        foreach (var key in keys)
        {
            if (key != null) UnfreezeEnemy(key);
        }
        frozenEnemies.Clear();
        frozenFreezables.Clear();
    }

    private void RefreezeOverlapping()
    {
        if (triggerCollider == null) return;

        float effectiveRadius = triggerCollider.radius * triggerCollider.transform.lossyScale.x;
        Collider2D[] hits = Physics2D.OverlapCircleAll(triggerCollider.transform.position, effectiveRadius);

        foreach (var hit in hits)
        {
            Health enemy = hit.GetComponentInParent<Health>();
            if (enemy != null && !enemy.IsDead)
            {
                FreezeEnemy(enemy.gameObject);
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
        ElectricEelController[] all = FindObjectsByType<ElectricEelController>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            Destroy(e.gameObject);
        }
    }
    private static bool isReloadingScene = false;

    public static void PrepareForSceneReload()
    {
        isReloadingScene = true;

        ElectricEelController[] all = FindObjectsByType<ElectricEelController>(FindObjectsSortMode.None);
        foreach (var eel in all)
        {
            eel.UnfreezeAll(); // properly restore every frozen enemy before scene teardown begins
        }
    }

}