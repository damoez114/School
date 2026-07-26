using UnityEngine;

// Bridges TripleHookConsumable.Activate() to whichever Roe the player clicks next.
public static class TripleHookTargeting
{
    private static bool isAwaitingTarget = false;
    private static float pendingDamageBonus = 0f;
    private static ConsumableSlot pendingSlot = null;

    public static bool IsAwaitingTarget => isAwaitingTarget;
    public static ConsumableSlot PendingSlot => pendingSlot;

    public static void BeginTargeting(float damageBonus)
    {
        isAwaitingTarget = true;
        pendingDamageBonus = damageBonus;
    }

    public static void SetPendingSlot(ConsumableSlot slot)
    {
        pendingSlot = slot;
    }

    public static bool TryConsumeTarget(out float damageBonus, out ConsumableSlot slot)
    {
        if (isAwaitingTarget)
        {
            damageBonus = pendingDamageBonus;
            slot = pendingSlot;
            isAwaitingTarget = false;
            pendingSlot = null;
            return true;
        }

        damageBonus = 0f;
        slot = null;
        return false;
    }

    public static void CancelTargeting()
    {
        isAwaitingTarget = false;
        pendingSlot = null;
    }
}