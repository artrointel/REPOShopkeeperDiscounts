# ShopDiscountBot

**Tune the vanilla shopkeeper discount ritual. Only the host installs the mod; friends can stay completely vanilla.**

Use REPOConfig's **Mods → ShopDiscountBot** settings on the host:

| Setting | Range | Default |
| --- | --- | --- |
| ChancePercent | 0–100% | 70% |
| AttemptsPerShop | 0–10 | 5 |

Attempts are shared by the entire lobby for each shop. Both successful and failed accepted scans consume an attempt. **0 attempts disables the ritual**; 0% chance always fails, and 100% always succeeds for valid items.

Settings apply to the next verdict/new request. Used attempts are not refunded when settings change, and an already accepted scan may finish. A new shop resets the allowance normally.

The shopkeeper keeps its native animations, sounds, discount amounts, price synchronization, failure consequences, and checkout. Failed scans can still damage or destroy items. No automatic shop-wide discount or free upgrade is added.

## Installation

Install this package on the host with a Thunderstore-compatible mod manager. It installs BepInExPack and the community-maintained REPOConfig; REPOConfig installs MenuLib. Joining players do not need any of these mods.

Manual config: `BepInEx/config/artrointel.ShopDiscountBot.cfg`.

Targets R.E.P.O. 0.5.0 (The Sunstone Update). Initial build: automated tests pass; in-game UI and host/unmodded-client testing are still pending.
