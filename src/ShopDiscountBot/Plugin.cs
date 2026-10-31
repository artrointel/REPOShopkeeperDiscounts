using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ShopDiscountBot;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "artrointel.ShopDiscountBot";
    public const string PluginName = "ShopDiscountBot";
    public const string PluginVersion = "0.1.0";
    internal static DiscountSettings Settings { get; private set; } = null!;
    private Harmony? _harmony;

    private void Awake()
    {
        Settings = new DiscountSettings(Config);
        _harmony = new Harmony(PluginGuid);
        try
        {
            _ = ShopBindings.Attempts;
            _harmony.PatchAll();
            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded. Host settings: {Settings.ChancePercent.Value}% chance, {Settings.AttemptsPerShop.Value} attempts per shop.");
        }
        catch (Exception exception)
        {
            _harmony.UnpatchSelf();
            Logger.LogError($"Initialization failed; all ShopDiscountBot hooks were removed: {exception}");
            enabled = false;
        }
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();
}
