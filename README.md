# ShopDiscountBot

Configure the **vanilla shopkeeper item-discount ritual** in R.E.P.O. 0.5.0. Only the host needs this mod and its configuration UI dependencies; joining players can remain completely vanilla.

Project/repository folder: `REPOShopDiscountBot`. Plugin/package name: `ShopDiscountBot`. [Source repository](https://github.com/artrointel/REPOShopDiscountBot).

## Settings

Open **Mods → ShopDiscountBot** in REPOConfig on the host. Settings are also saved to `BepInEx/config/artrointel.ShopDiscountBot.cfg`.

| Setting | Range | Default | Meaning |
| --- | --- | --- | --- |
| `ChancePercent` | 0–100, integer | 70 | Positive verdict chance for each valid shopkeeper scan. 0 always fails; 100 always succeeds. Native chance is 1/3, approximately 33.33%. |
| `AttemptsPerShop` | 0–10, integer | 5 | Total scan attempts shared by the entire lobby for each shop. 0 disables discount attempts. This is not a per-player allowance or a successful-discount count. |

Changes apply to the next verdict/new request without restarting the game. An accepted scan may finish after the limit is reduced. Used attempts are never refunded when settings change. A new shop receives a fresh allowance through the native shopkeeper instance.

The native presentation gesture, discount amount, minimum reduction, item reactions, failure consequences, effects, price synchronization, and checkout remain unchanged. A failed verdict can still lead to native item damage/destruction. This mod does not discount every shop item automatically or add free upgrades.

## Dependencies and host-only behavior

Install on the host:

- BepInExPack 5.4.2305
- [REPOConfig 1.2.7 by REPOModding](https://thunderstore.io/c/repo/p/REPOModding/REPOConfig/)
- REPOConfig's MenuLib 2.6.0 dependency

The old nickklmao REPOConfig listing is deprecated; use the community-maintained REPOModding package. Both settings are ordinary BepInEx ConfigEntry<int> values with AcceptableValueRange, as documented by REPOConfig. No custom UI, RPC, prefab, or Photon stream is introduced. A modded non-host keeps native decisions; its local settings do not control the lobby.

## Build

The repository includes an ignored local .NET 9 SDK and package cache for this workstation. On another machine, install a compatible SDK or supply `.dotnet/dotnet.exe`. Copy `Directory.Repo.props.example` to the ignored `Directory.Repo.props` and set the game and test profile paths.

```powershell
$env:DOTNET_CLI_HOME = "$PWD\.dotnet-home"
$env:NUGET_PACKAGES = "$PWD\.nuget\packages"
.\.dotnet\dotnet.exe restore src/ShopDiscountBot/ShopDiscountBot.csproj
.\.dotnet\dotnet.exe build src/ShopDiscountBot/ShopDiscountBot.csproj -c Debug -p:CopyFilesToPluginOutputDirectoryOnBuild=false
.\.dotnet\dotnet.exe build src/ShopDiscountBot/ShopDiscountBot.csproj -c Release -p:CopyFilesToPluginOutputDirectoryOnBuild=false
.\.dotnet\dotnet.exe run --project tests/ShopDiscountBot.Checks/ShopDiscountBot.Checks.csproj
```

Builds reference installed R.E.P.O. 0.5.0 / Unity 6 assemblies directly. Game assemblies and test libraries are never included in the release ZIP. No game files are modified.

## Test profile and VS Code

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Setup-TestProfile.ps1 -NoRestore
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Start-RepoDev.ps1
```

The isolated `ShopDiscountBot-Test` profile contains BepInEx, ShopDiscountBot, REPOConfig, and MenuLib. Setup does not alter other profiles or start/stop a game. Close any running R.E.P.O. instance before launching.

- `Ctrl+Shift+B`: build and deploy Debug to the dedicated profile.
- `ShopDiscountBot: Start game`: build, deploy, and launch that profile.
- `F5`: launch with the Mono debugger and attach at `127.0.0.1:10000`.
- `ShopDiscountBot: Watch BepInEx log`: follow the dedicated profile log.

No development item spawns, F8 probes, or in-game test shortcuts are included in either build configuration.

## Packaging

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Pack-Thunderstore.ps1 -NoRestore
```

Creates `artifacts/ShopDiscountBot-0.1.0.zip` from Release without deploying. The included simple development icon can be replaced before public release. The manifest links to the source repository. The package has not been published to Thunderstore.

See [the implementation notes](planning/README.md) and [manual test plan](planning/test-plan.md). Automated checks do not replace actual host/unmodded-client gameplay or verification of the settings UI in the game.
