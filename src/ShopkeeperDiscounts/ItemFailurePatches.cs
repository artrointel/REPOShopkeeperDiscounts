using HarmonyLib;
using UnityEngine;

namespace ShopkeeperDiscounts;

// Host-local bookkeeping only. It has no update loop, RPC or saved item entry.
internal sealed class ItemFailureTracker : MonoBehaviour
{
    internal readonly FailedDiscountCounter Counter = new();
}

[HarmonyPatch(typeof(ShopKeeper), "ItemShownSet")]
internal static class ItemFailureVerdictPatch
{
    [HarmonyPrefix]
    private static void Prefix(PhysGrabObject _physGrabObject, int _timesShown, bool _disliked)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || _physGrabObject == null)
        {
            return;
        }
        ItemFailureTracker tracker = _physGrabObject.GetComponent<ItemFailureTracker>();
        if (tracker == null)
        {
            tracker = _physGrabObject.gameObject.AddComponent<ItemFailureTracker>();
        }
        tracker.Counter.ObserveVerdict(_timesShown, _disliked, Plugin.Settings.BreakAfterFailures.Value);
    }
}

[HarmonyPatch(typeof(ShopKeeper), "ItemDislikeAffectApply")]
internal static class ItemFailureDestructionPatch
{
    [HarmonyPrefix]
    private static bool Prefix(PhysGrabObject _physGrabObject)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || _physGrabObject == null)
        {
            return true;
        }
        ItemFailureTracker tracker = _physGrabObject.GetComponent<ItemFailureTracker>();
        // Below the threshold, keep the stock rejection/shake/toss animation
        // but do not broadcast the native exploding-item effect. At the
        // threshold, let the original method and all its native RPCs run.
        return tracker == null || tracker.Counter.BreakAllowed;
    }
}
