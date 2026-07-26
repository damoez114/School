using UnityEngine;

public class ConsumableUseTagAppear : MonoBehaviour
{
    public GameObject useTag;
    public GameObject cancelTag; // new — assign a second small UI tag with its own "Cancel" button

    [Header("Sound")]
    [SerializeField] private AudioClip useSound;
    [SerializeField] private AudioClip cancelSound;

    private bool isActive;
    private bool isAwaitingCancel;
    private ConsumableItem currentItem;
    private ConsumableSlot currentSlot;

    public bool IsShowing => isActive || isAwaitingCancel;

    public void ShowTag(ConsumableItem item, ConsumableSlot slot)
    {
        currentItem = item;
        currentSlot = slot;
        isActive = true;
        useTag.SetActive(true);
    }

    public void HideTag()
    {
        isActive = false;
        useTag.SetActive(false);
    }

    public void ToggleTag(ConsumableItem item, ConsumableSlot slot)
    {
        if (isAwaitingCancel) return; // worm's already in use — ignore slot clicks until cancel/consume resolves

        if (isActive)
            HideTag();
        else
            ShowTag(item, slot);
    }

    // Wired to the Confirm button inside `useTag`
    public void ConfirmUse()
    {
        if (currentItem == null || currentSlot == null) return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(useSound);

        currentItem.Activate();
        HideTag();

        if (FishGrowthTargeting.IsAwaitingTarget)
        {
            FishGrowthTargeting.SetPendingSlot(currentSlot);
            isAwaitingCancel = true;
            cancelTag.SetActive(true);
        }
        else if (TripleHookTargeting.IsAwaitingTarget)
        {
            TripleHookTargeting.SetPendingSlot(currentSlot);
            isAwaitingCancel = true;
            cancelTag.SetActive(true);
        }
        else
        {
            currentSlot.Clear();
        }
    }

    public void CancelUse()
    {
        if (!isAwaitingCancel) return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(cancelSound);

        FishGrowthTargeting.CancelTargeting();
        TripleHookTargeting.CancelTargeting(); // harmless no-op if it wasn't the active one
        HideCancelTagVisual();
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    // Called by Roe when a target is successfully consumed, so the cancel tag
    // disappears without re-cancelling an already-completed growth
    public void HideCancelTagVisual()
    {
        isAwaitingCancel = false;
        cancelTag.SetActive(false);
    }
}