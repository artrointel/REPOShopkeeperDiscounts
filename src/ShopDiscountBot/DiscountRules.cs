using System;

namespace ShopDiscountBot;

internal static class DiscountRules
{
    internal static bool Succeeds(int chancePercent, int roll) =>
        roll >= 0 && roll < 100 && roll < Math.Max(0, Math.Min(100, chancePercent));
}
