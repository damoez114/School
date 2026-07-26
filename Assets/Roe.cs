using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Roe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static int fishCount = 0;

    public Image image;

    [HideInInspector] public Transform parentAfterDrag;

    [SerializeField] private GameObject bluegillPrefab;

    public string itemID;

    private Canvas canvas;
    [Header("Sound")]
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip dragSound;
    [SerializeField] private AudioClip sellSound;
    public GameObject FishPrefab => bluegillPrefab;

    // ADD THESE TWO LINES:
    [SerializeField] private RoeItem sourceItem;
    public RoeItem SourceItem
    {
        get => sourceItem;
        set => sourceItem = value;
    }

    public int fishValue = 1;
    [Header("Growth (Worm consumable)")]
    [SerializeField] private float growthMultiplier = 1f;
    public float GrowthMultiplier => growthMultiplier;
    [SerializeField] private GameObject grownIcon;

    public void SetGrowthMultiplier(float multiplier)
    {
        growthMultiplier = multiplier;

        if (grownIcon != null)
            grownIcon.SetActive(multiplier > 1f);
    }

    // =========================
    // PLACEMENT STATE (deploy-without-consuming)
    // =========================
    [Header("Placement Visual")]
    [SerializeField] private Color placedTint = new Color(0.55f, 0.55f, 0.55f, 0.65f);
    private Color originalColor = Color.white;

    public bool IsPlaced { get; private set; }

    // Marks whether this card has already been used to deploy a fish this
    // attempt. When true, dropping it onto the field just snaps it back
    // instead of spawning another fish. Rearranging/dragging between slots
    // is unaffected — only the "drop onto the field" path checks this.
    public void SetPlaced(bool placed)
    {
        IsPlaced = placed;

        if (image != null)
            image.color = placed ? placedTint : originalColor;

        if (HotbarManager.Instance != null)
            HotbarManager.Instance.UpdateRoeCount();
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (image != null)
            originalColor = image.color;
    }

    // =========================
    // CLICK — consume a pending Worm target
    // =========================
    [Header("Triple Hook (damage bonus)")]
    [SerializeField] private float damageBonus = 0f;
    public float DamageBonus => damageBonus;
    [SerializeField] private GameObject hookedIcon;

    public void SetDamageBonus(float bonus)
    {
        damageBonus = bonus;

        if (hookedIcon != null)
            hookedIcon.SetActive(bonus > 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(clickSound);

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();

        parentAfterDrag = transform.parent;
        if (FishGrowthTargeting.TryConsumeTarget(out float growthPercent, out ConsumableSlot growthSlot))
        {
            SetGrowthMultiplier(growthMultiplier + growthPercent);

            if (growthSlot != null)
            {
                if (growthSlot.useTag != null)
                    growthSlot.useTag.HideCancelTagVisual();

                growthSlot.Clear();
            }

            HotbarManager.Instance.SaveHotbar();

            return;
        }

        if (TripleHookTargeting.TryConsumeTarget(out float bonus, out ConsumableSlot hookSlot))
        {
            SetDamageBonus(damageBonus + bonus);

            if (hookSlot != null)
            {
                if (hookSlot.useTag != null)
                    hookSlot.useTag.HideCancelTagVisual();

                hookSlot.Clear();
            }

            HotbarManager.Instance.SaveHotbar();
        }
    }

    // =========================
    // DRAG START
    // =========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(dragSound);

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();

        parentAfterDrag = transform.parent;

        HotbarSlot oldSlot = parentAfterDrag.GetComponent<HotbarSlot>();
        if (oldSlot != null)
            oldSlot.currentRoe = null;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        image.raycastTarget = false;
    }

    // =========================
    // DRAG
    // =========================
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // Snaps this card back into a hotbar slot without going through
    // HotbarSlot.SetRoe (which would force an unconditional save). Mirrors
    // the existing "blocked placement" snap-back pattern below.
    private void SnapBackToSlot(HotbarSlot slot)
    {
        Transform target = slot != null ? slot.transform : parentAfterDrag;
        transform.SetParent(target);
        transform.localPosition = Vector3.zero;

        if (slot != null)
            slot.currentRoe = this;
    }

    // Instantiates the fish and applies growth/damage/rarity bonuses. Shared
    // by the manual drag-drop path and TryAutoDeploy (used by Replay Last
    // Round). Does NOT touch placement state or the hotbar — callers handle
    // that, since the two callers need slightly different behavior around it.
    private GameObject SpawnFishAt(Vector3 worldPos)
    {
        GameObject spawnedFish = Instantiate(bluegillPrefab, worldPos, Quaternion.identity);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(splashSound);

        FishStats spawnedStats = spawnedFish.GetComponent<FishStats>();
        if (spawnedStats != null)
        {
            if (growthMultiplier != 1f)
                spawnedStats.SetGrowthMultiplier(growthMultiplier);

            if (damageBonus != 0f)
                spawnedStats.AddTripleHookBonus(damageBonus);

            // Permanent rarity-wide bonus from Simple/Weighted/Jighead Hook consumables
            if (sourceItem != null)
            {
                float rarityBonus = RarityDamageBonusManager.GetBonus(sourceItem.ItemRarity);
                if (rarityBonus != 0f)
                    spawnedStats.AddTripleHookBonus(rarityBonus);
            }
        }

        fishCount += fishValue;

        return spawnedFish;
    }

    // Programmatic deploy used by RoundPlacementManager.ReplaySavedPlacements()
    // to recreate a saved layout, rotation included. Mirrors the guard clauses
    // in OnEndDrag's world-drop path but returns a bool instead of snapping
    // back on failure, since there's no drag in progress to snap back from.
    public bool TryAutoDeploy(Vector3 worldPos, Quaternion rotation)
    {
        if (IsPlaced || ShopManager.IsShopOpen)
            return false;

        if (bluegillPrefab != null && bluegillPrefab.GetComponent<PufferfishController>() != null
            && PufferfishController.IsActiveInScene)
            return false;

        if (bluegillPrefab != null && bluegillPrefab.GetComponent<HalibutController>() != null
            && HalibutController.IsActiveInScene)
            return false;

        if (bluegillPrefab != null && bluegillPrefab.GetComponent<ElectricEelController>() != null
            && ElectricEelController.IsActiveInScene)
            return false;

        GameObject spawnedFish = SpawnFishAt(worldPos);
        spawnedFish.transform.rotation = rotation;

        // Some fish scripts set their own facing/rotation in Start(), which
        // runs AFTER this line on the same frame (Instantiate only runs
        // Awake synchronously). If that's stomping our rotation, this
        // reapplies it one frame later so it sticks.
        StartCoroutine(ReapplyRotationNextFrame(spawnedFish.transform, rotation));

        SetPlaced(true);
        HotbarManager.roeUsedThisAttempt = true;

        return true;
    }

    private IEnumerator ReapplyRotationNextFrame(Transform target, Quaternion rotation)
    {
        yield return null;

        if (target != null)
            target.rotation = rotation;
    }

    // =========================
    // DROP
    // =========================
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        // Check sell bin first — takes priority over slot/world drop
        SellBin sellBin = eventData.pointerEnter ? eventData.pointerEnter.GetComponentInParent<SellBin>() : null;

        if (sellBin != null)
        {
            SellRoe();
            return;
        }

        HotbarSlot targetSlot =
            eventData.pointerEnter ?
            eventData.pointerEnter.GetComponentInParent<HotbarSlot>() :
            null;

        HotbarSlot oldSlot = parentAfterDrag.GetComponent<HotbarSlot>();

        // =========================
        // DROP INTO WORLD (deploy)
        // =========================
        if (targetSlot == null)
        {
            if (ShopManager.IsShopOpen)
            {
                SnapBackToSlot(oldSlot);
                return;
            }

            // Already used this attempt — just snap back, don't deploy again
            if (IsPlaced)
            {
                SnapBackToSlot(oldSlot);
                return;
            }

            // Block placing a second Pufferfish — snap back to original slot instead
            if (bluegillPrefab != null && bluegillPrefab.GetComponent<PufferfishController>() != null
                && PufferfishController.IsActiveInScene)
            {
                SnapBackToSlot(oldSlot);
                return;
            }
            if (bluegillPrefab != null && bluegillPrefab.GetComponent<HalibutController>() != null && HalibutController.IsActiveInScene)
            {
                SnapBackToSlot(oldSlot);
                return;
            }
            if (bluegillPrefab != null && bluegillPrefab.GetComponent<ElectricEelController>() != null && ElectricEelController.IsActiveInScene)
            {
                SnapBackToSlot(oldSlot);
                return;
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            GameObject spawnedFish = SpawnFishAt(worldPos);

            // Snap back into the hotbar instead of being destroyed, and grey
            // it out to show it's already been deployed this attempt.
            SnapBackToSlot(oldSlot);
            SetPlaced(true);

            HotbarManager.roeUsedThisAttempt = true;

            // Remember this placement so it can be replayed on future tries
            // this round (see RoundPlacementManager). Rotation isn't known
            // yet here — the player can still rotate the fish with
            // FishRotateHandle before pressing Play — so we pass along the
            // fish's transform and its final rotation gets read later, at
            // save time.
            if (RoundPlacementManager.Instance != null)
                RoundPlacementManager.Instance.RecordPlacement(itemID, worldPos, growthMultiplier, damageBonus, spawnedFish.transform);

            return;
        }

        // =========================
        // MOVE INTO SLOT (rearrange — allowed even when placed)
        // =========================
        Roe targetRoe = targetSlot.currentRoe;

        if (oldSlot != null)
            oldSlot.Clear();

        transform.SetParent(targetSlot.transform);
        transform.localPosition = Vector3.zero;

        targetSlot.currentRoe = this;

        if (targetRoe != null && targetRoe != this)
        {
            targetRoe.transform.SetParent(oldSlot.transform);
            targetRoe.transform.localPosition = Vector3.zero;

            oldSlot.currentRoe = targetRoe;
        }

        HotbarManager.Instance.SaveHotbar();

    }
    private void SellRoe()
    {
        int cost = HotbarManager.Instance.GetCostByID(itemID);
        int sellValue = Mathf.RoundToInt(cost / 3f);

        ShellManager.Instance.AddShells(sellValue);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(sellSound);

        HotbarSlot oldSlot = parentAfterDrag.GetComponent<HotbarSlot>();
        if (oldSlot != null)
            oldSlot.Clear();

        Destroy(gameObject);

        HotbarManager.Instance.SaveHotbar();

        Debug.Log($"Sold {itemID} for {sellValue} shells");
    }

}