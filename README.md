# ShopkeeperDiscounts

Configure the **vanilla shopkeeper item-discount ritual** in R.E.P.O. 0.5.0. Only the host needs this mod and its configuration UI dependencies; joining players can remain completely vanilla.

Project/repository folder: `REPOShopkeeperDiscounts`. Plugin/package name: `ShopkeeperDiscounts`. [Source repository](https://github.com/artrointel/REPOShopkeeperDiscounts).

## Settings

Open **Mods → ShopkeeperDiscounts** in REPOConfig on the host. Settings are also saved to `BepInEx/config/artrointel.ShopkeeperDiscounts.cfg`.

| Setting | Range | Default | Meaning |
| --- | --- | --- | --- |
| `ChancePercent` | 0–100, integer | 70 | Positive verdict chance for each valid shopkeeper scan. 0 always fails; 100 always succeeds. Native chance is 1/3, approximately 33.33%. |
| `AttemptsPerShop` | 0–10, integer | 5 | Total scan attempts shared by the entire lobby for each shop. 0 disables discount attempts. This is not a per-player allowance or a successful-discount count. |
| `BreakAfterFailures` | 1-5, integer | 1 | Cumulative failed verdicts on the same physical item before the native destruction effect is allowed. Shared by all players. Successful discounts do not reset it. |

Changes apply to the next verdict/new request without restarting the game. An accepted scan may finish after the limit is reduced. Used attempts are never refunded when settings change. A new shop receives a fresh allowance through the native shopkeeper instance.

Failure counts belong to individual physical items, not item types or players. For example, with `BreakAfterFailures=3`, failure, success, failure, failure on the same item allows the native destruction effect on its third failure. A different copy has its own count. Counts end when that item is destroyed/replaced and are not persisted in save files. The break threshold is captured at each verdict; changing it does not retroactively change an ongoing rejection.

The native presentation gesture, discount amount, minimum reduction, item reactions, price synchronization, and checkout remain unchanged. Before the failure threshold, rejection/shaking/tossing still play, but the shopkeeper does not apply the native exploding-item effect. At the threshold, that original effect and its timing resume. The setting controls shopkeeper rejection destruction, not general protection from unrelated damage. This mod does not discount every shop item automatically or add free upgrades.

## Dependencies and host-only behavior

Install on the host:

- BepInExPack 5.4.2305
- [REPOConfig 1.2.7 by REPOModding](https://thunderstore.io/c/repo/p/REPOModding/REPOConfig/)
- REPOConfig's MenuLib 2.6.0 dependency

The old nickklmao REPOConfig listing is deprecated; use the community-maintained REPOModding package. All three settings are ordinary BepInEx ConfigEntry<int> values with AcceptableValueRange, as documented by REPOConfig. No custom UI, RPC, prefab, or Photon stream is introduced. A modded non-host keeps native decisions; its local settings do not control the lobby.

## Build

The repository includes an ignored local .NET 9 SDK and package cache for this workstation. On another machine, install a compatible SDK or supply `.dotnet/dotnet.exe`. Copy `Directory.Repo.props.example` to the ignored `Directory.Repo.props` and set the game and test profile paths.

```powershell
$env:DOTNET_CLI_HOME = "$PWD\.dotnet-home"
$env:NUGET_PACKAGES = "$PWD\.nuget\packages"
.\.dotnet\dotnet.exe restore src/ShopkeeperDiscounts/ShopkeeperDiscounts.csproj
.\.dotnet\dotnet.exe build src/ShopkeeperDiscounts/ShopkeeperDiscounts.csproj -c Debug -p:CopyFilesToPluginOutputDirectoryOnBuild=false
.\.dotnet\dotnet.exe build src/ShopkeeperDiscounts/ShopkeeperDiscounts.csproj -c Release -p:CopyFilesToPluginOutputDirectoryOnBuild=false
.\.dotnet\dotnet.exe run --project tests/ShopkeeperDiscounts.Checks/ShopkeeperDiscounts.Checks.csproj
```

Builds reference installed R.E.P.O. 0.5.0 / Unity 6 assemblies directly. Game assemblies and test libraries are never included in the release ZIP. No game files are modified.

## Test profile and VS Code

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Setup-TestProfile.ps1 -NoRestore
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Start-RepoDev.ps1
```

The isolated `ShopkeeperDiscounts-Test` profile contains BepInEx, ShopkeeperDiscounts, REPOConfig, and MenuLib. Setup does not alter other profiles or start/stop a game. Close any running R.E.P.O. instance before launching.

- `Ctrl+Shift+B`: build and deploy Debug to the dedicated profile.
- `ShopkeeperDiscounts: Start game`: build, deploy, and launch that profile.
- `F5`: launch with the Mono debugger and attach at `127.0.0.1:10000`.
- `ShopkeeperDiscounts: Watch BepInEx log`: follow the dedicated profile log.

No development item spawns, F8 probes, or in-game test shortcuts are included in either build configuration.

## Packaging

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Pack-Thunderstore.ps1 -NoRestore
```

Creates `artifacts/ShopkeeperDiscounts-0.1.2.zip` from Release without deploying. The package includes a 256x256 icon and links to the source repository. Version 0.1.1 is the initial public release package; earlier versions were development builds. Uploading to Thunderstore is a separate manual step.

See [the implementation notes](planning/README.md) and [manual test plan](planning/test-plan.md). Automated checks do not replace actual host/unmodded-client gameplay or verification of the settings UI in the game.

## Release validation

The 0.1.2 release has 375 automated checks covering discount rules, configuration bounds/persistence, per-item failure counters, and patch compatibility with the installed game assemblies. The user confirmed in-game operation after the legacy development DLL was removed. A dedicated host/unmodded-client multiplayer test has not been recorded.
