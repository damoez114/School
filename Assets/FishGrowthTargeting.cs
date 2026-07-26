using UnityEngine;

// Bridges WormConsumable.Activate() to whichever Roe the player clicks next.
public static class FishGrowthTargeting
{
    private static bool isAwaitingTarget = false;
    private static float pendingGrowthPercent = 0f;
    private static ConsumableSlot pendingSlot = null;

    public static bool IsAwaitingTarget => isAwaitingTarget;
    public static ConsumableSlot PendingSlot => pendingSlot;

    public static void BeginTargeting(float growthPercent)
    {
        isAwaitingTarget = true;
        pendingGrowthPercent = growthPercent;
    }

    public static void SetPendingSlot(ConsumableSlot slot)
    {
        pendingSlot = slot;
    }

    public static bool TryConsumeTarget(out float growthPercent, out ConsumableSlot slot)
    {
        if (isAwaitingTarget)
        {
            growthPercent = pendingGrowthPercent;
            slot = pendingSlot;
            isAwaitingTarget = false;
            pendingSlot = null;
            return true;
        }

        growthPercent = 0f;
        slot = null;
        return false;
    }

    // Fires when the player clicks the worm icon again while it's armed
    public static void CancelTargeting()
    {
        isAwaitingTarget = false;
        pendingSlot = null;
    }
}