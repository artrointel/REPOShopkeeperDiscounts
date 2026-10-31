using UnityEngine;

namespace ShopDiscountBot;

internal static class DiscountPolicy
{
    internal static int AttemptLimit() => SemiFunc.IsMasterClientOrSingleplayer()
        ? Plugin.Settings.AttemptsPerShop.Value : 3;

    // Preserve the native Range(0, 3) == 0 expression and all native bookkeeping;
    // only substitute the draw. No other random calls are patched.
    internal static int DrawVerdict(int minimum, int maximum)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            return Random.Range(minimum, maximum);
        }
        return DiscountRules.Succeeds(Plugin.Settings.ChancePercent.Value, Random.Range(0, 100)) ? 0 : 1;
    }
}
