using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace ShopkeeperDiscounts;

internal static class ShopBindings
{
    internal static readonly FieldInfo Attempts = AccessTools.DeclaredField(typeof(ShopKeeper), "itemDiscountAttempts")
        ?? throw new System.MissingFieldException(typeof(ShopKeeper).FullName, "itemDiscountAttempts");

    internal static int ReadAttempts(ShopKeeper keeper) => (int)Attempts.GetValue(keeper);
    internal static void RestoreAttempts(ShopKeeper keeper, int value) => Attempts.SetValue(keeper, value);
}

[HarmonyPatch]
internal static class AttemptLimitPatch
{
    internal static readonly string[] MethodNames =
    {
        "ItemPresentationTargetValid", "ItemNoticeStart", "TryStartItemDiscount", "ItemDenyPhysics"
    };

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (string name in MethodNames)
        {
            yield return AccessTools.DeclaredMethod(typeof(ShopKeeper), name)
                ?? throw new System.MissingMethodException(typeof(ShopKeeper).FullName, name);
        }
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod) =>
        DiscountIlRewriter.AttemptLimit(instructions,
            AccessTools.DeclaredField(typeof(ShopKeeper), "itemDiscountAttempts"),
            AccessTools.DeclaredMethod(typeof(DiscountPolicy), nameof(DiscountPolicy.AttemptLimit)),
            __originalMethod.Name);
}

[HarmonyPatch(typeof(ShopKeeper), "ItemVerdictPositiveGet")]
internal static class VerdictChancePatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
        DiscountIlRewriter.Verdict(instructions,
            AccessTools.DeclaredMethod(typeof(UnityEngine.Random), "Range", new[] { typeof(int), typeof(int) }),
            AccessTools.DeclaredMethod(typeof(DiscountPolicy), nameof(DiscountPolicy.DrawVerdict)));
}

[HarmonyPatch(typeof(ShopKeeper), "UpdateStateRPC")]
internal static class HostAttemptCounterPatch
{
    [HarmonyPrefix]
    private static void Prefix(ShopKeeper __instance, out int __state)
    {
        __state = SemiFunc.IsMasterClientOrSingleplayer() ? ShopBindings.ReadAttempts(__instance) : -1;
    }

    [HarmonyPostfix]
    private static void Postfix(ShopKeeper __instance, int __state)
    {
        // Native UpdateStateRPC clamps the counter to 3 on every local echo.
        // Keep the host's authoritative spent count; vanilla peers may retain
        // their native clamp because only the host accepts and judges attempts.
        if (__state >= 0)
        {
            ShopBindings.RestoreAttempts(__instance, __state);
        }
    }
}
