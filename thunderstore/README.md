# ShopkeeperDiscounts

**More chances to bargain. Fewer broken purchases. Only the host installs the mod; friends can stay completely vanilla.**

Customize the shopkeeper discount ritual introduced in R.E.P.O. 0.5.0: choose the success chance, the shared attempts per shop, and how many failed attempts each item can survive. Defaults give you a **70% success chance and 5 attempts per shop**, with native first-failure destruction unless you raise the break threshold.

Use REPOConfig's **Mods → ShopkeeperDiscounts** settings on the host:

| Setting | Range | Default |
| --- | --- | --- |
| ChancePercent | 0–100% | 70% |
| AttemptsPerShop | 0–10 | 5 |
| BreakAfterFailures | 1-5 failures per item | 1 |

Attempts are shared by the entire lobby for each shop. Both successful and failed accepted scans consume an attempt. **0 attempts disables the ritual**; 0% chance always fails, and 100% always succeeds for valid items.

Settings apply to the next verdict/new request. Used attempts are not refunded when settings change, and an already accepted scan may finish. A new shop resets the allowance normally. A break-threshold change applies to the next verdict; it does not alter a rejection already decided.

The shopkeeper keeps its native animations, sounds, discount amounts, price synchronization, and checkout. `BreakAfterFailures` delays the native failure destruction effect until the same physical item accumulates that many failed verdicts. Success does not reset the count; different item copies are independent. Before the threshold, the normal rejection/shake/toss still plays. At the threshold, the original destruction effect and timing are used. Default 1 preserves first-failure destruction. This does not protect against unrelated damage or persist counts across replacement/save reloads. No automatic shop-wide discount or free upgrade is added.

## Installation

Install this package on the host with a Thunderstore-compatible mod manager. It installs BepInExPack and the community-maintained REPOConfig; REPOConfig installs MenuLib. Joining players do not need any of these mods.

Manual config: `BepInEx/config/artrointel.ShopkeeperDiscounts.cfg`.

Targets R.E.P.O. 0.5.0 (The Sunstone Update).
