# ShopkeeperDiscounts implementation contract

Target: R.E.P.O. 0.5.0, Steam build 26294889, Unity 6000.3.17. Fixed host; no host migration feature.

## Native behavior

ShopKeeper.ItemVerdictPositiveGet validates host authority and the item, draws Random.Range(0, 3) == 0, and invokes ItemShownSet for native reaction bookkeeping and synchronization. The original shopkeeper stores a lobby-wide itemDiscountAttempts counter, incremented when TryStartItemDiscount accepts a scan. It has four comparisons against a fixed limit of three, plus a 0–3 clamp in UpdateStateRPC.

## Changes

- Replace only the verdict's integer random draw with a configured 0–99 draw. Keep native target validation, ItemShownSet, positive/negative state transitions, and RPCs intact.
- Replace only the constant immediately following the native counter field load in ItemPresentationTargetValid, ItemNoticeStart, TryStartItemDiscount, and ItemDenyPhysics. Each rewrite must match exactly once. Branch labels and exception blocks stay attached to the rewritten instruction.
- Read the configured limit on the host at the decision point; modded clients retain three. Keep the native increment and new-shop object lifetime.
- Preserve the host's existing authoritative counter around the local UpdateStateRPC echo so its native 0–3 clamp cannot turn a larger allowance into endless retries. Never restore or override an unmodded remote client's private state. Native state RPCs still tell those clients which outcome to present.
- Register three integer BepInEx settings: ChancePercent (0-100, default 70), AttemptsPerShop (0-10, default 5), and BreakAfterFailures (1-5, default 1). Read current values at each decision. Reducing the limit does not undo an accepted scan or refund prior attempts.
- Remove every installed hook if initialization fails or expected native IL no longer matches.

## Compatibility

All price changes, discount popups, reactions, item destruction, and late-join snapshots remain the game's native implementation. No custom RPC, PhotonStream, network prefab, shop object, upgrade reward, or UI is added. Only the host needs the mod plus REPOConfig/MenuLib to edit settings in-game. Client configuration cannot change host outcomes.

Other mods patching the same shopkeeper methods may conflict. Verify alongside RaiseToUpgrade: holding an item near the shopkeeper can invoke both independent rituals. This mod does not suppress either feature.

## Validation

The standalone checks cover all integer chance boundaries, actual BepInEx range clamping and persistence, executable rewritten comparisons for every requested attempt limit, live game IL rewritten by the production transformer, host-counter preservation by the production postfix, and the native RPC/member contract. Unity gameplay and actual configuration menu interaction remain manual checks.

Primary references:
- https://github.com/REPOModding/REPOConfig
- https://thunderstore.io/c/repo/p/REPOModding/REPOConfig/
- Installed Assembly-CSharp.dll / ShopKeeper implementation

## Per-item cumulative failure threshold (0.1.1)

- Attach one host-local ItemFailureTracker to each physical item when ItemShownSet records a verdict. Use the native TimesShown sequence to ignore duplicate/stale callbacks. Track only rejected verdicts; positive verdicts leave the accumulated failure count intact.
- Counters are shared by all players using that physical object and are independent for different copies of the same item type. They follow the object's lifetime and do not enter saved item stats.
- Freeze whether destruction is allowed at the verdict, using the threshold active at that moment. Changing the setting during the delayed rejection cannot retroactively alter the outcome.
- Gate only ShopKeeper.ItemDislikeAffectApply. Below threshold, do not send ItemDislikeAffectApplyRPC or create its exploding affect. Preserve the native rejection state machine, shake/toss, and price behavior. At/above threshold, execute the original method and native RPCs unchanged.
- Default 1 preserves the native first-failure destruction path. The threshold does not grant general indestructibility, suppress unrelated bombs, or modify purchases. This mod adds no custom RPC, prefab, stream, or client component.
