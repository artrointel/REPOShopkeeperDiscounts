using BepInEx.Configuration;

namespace ShopkeeperDiscounts;

internal sealed class DiscountSettings
{
    internal ConfigEntry<int> ChancePercent { get; }
    internal ConfigEntry<int> AttemptsPerShop { get; }
    internal ConfigEntry<int> BreakAfterFailures { get; }

    internal DiscountSettings(ConfigFile config)
    {
        ChancePercent = config.Bind("Discounts", "ChancePercent", 70,
            new ConfigDescription("Host only. Chance of a positive shopkeeper discount verdict, in percent. 0 always fails; 100 always succeeds for a valid item. Applies to the next verdict. Vanilla is approximately 33.33%.",
                new AcceptableValueRange<int>(0, 100)));
        AttemptsPerShop = config.Bind("Discounts", "AttemptsPerShop", 5,
            new ConfigDescription("Host only. Total discount attempts shared by the entire lobby per shop, not successful discounts or attempts per player. 0 disables attempts. Changes affect new requests without refunding spent attempts; an already accepted scan may finish.",
                new AcceptableValueRange<int>(0, 10)));
        BreakAfterFailures = config.Bind("Discounts", "BreakAfterFailures", 1,
            new ConfigDescription("Host only. Cumulative failed discount verdicts on the same physical item before the native destruction effect is applied. Shared by all players; successful discounts do not reset the failure count. Applies to the next verdict. Default 1 preserves native first-failure destruction; 1-5 allowed.",
                new AcceptableValueRange<int>(1, 5)));
    }
}
