# ShopkeeperDiscounts manual test plan

1. Launch the dedicated profile and verify ShopkeeperDiscounts, REPOConfig, and MenuLib load without errors. Open Mods → ShopkeeperDiscounts and confirm ChancePercent is bounded to 0–100 and AttemptsPerShop to 0–10. Save, restart, and confirm persistence.
2. Set ChancePercent=100 and AttemptsPerShop=10. Present valid, non-free shop items ten times. The first ten accepted scans must succeed; the eleventh request must be denied. In particular, the fourth and later attempts must work and the allowance must stop at ten.
3. Set ChancePercent=0 and AttemptsPerShop=3. Every valid verdict must fail using the native negative reaction. Failed scans must consume attempts; no fourth scan should be accepted. Normal item destruction/damage rules remain in effect.
4. Set AttemptsPerShop=0. No discount scan should be accepted. Ordinary shop checkout and shopkeeper behavior should still work.
5. Use one or more attempts, reduce the limit below the used count, and verify new scans are denied. Raise it again and verify only the additional unspent allowance becomes available. Adjusting settings must not reset the counter.
6. Change settings during a scan. The current scan may finish under the new chance; reducing the attempt limit must not strand the state machine or cancel native cleanup. Verify the next request obeys the new limit.
7. Move to another shop and confirm the allowance resets. Death/reconnect within the same shop must not reset the shared allowance.
8. Install the mod and settings dependencies only on the host. Use an unmodded client to present items. Compare item prices, discount popups, scan/voice/particle effects, failures/destruction, and checkout on both screens. Both players share one allowance.
9. Test more than three attempts in a mixed lobby and join after several attempts. Native clients clamp their displayed private count to three, so confirm they still follow host-authoritative scan/denial states and actual prices correctly.
10. A modded client joining a vanilla host must not alter chance or the native three-attempt limit.
11. Verify invalid items, already-free items, destroyed items, and interrupted scans retain native validation and cleanup. An accepted scan consumes its attempt even if interrupted later.
12. If used with RaiseToUpgrade or other shopkeeper mods, check for overlapping gestures and Harmony conflicts. Test standalone first.

Automated verification is not proof of actual multiplayer feedback or settings-menu behavior. Capture host and vanilla-client logs and one positive/negative scan before public release.

## Per-item break counter

- Confirm REPOConfig shows BreakAfterFailures with default 1, lower bound 1 and upper bound 5, and persists the selection after restart.
- With chance 0, ten attempts and threshold 3, fail on item A twice. Each rejection must finish normally, with no exploding affect or destruction caused by that rejection. Fail a third time on A and verify the native effect is applied and destroys it using stock timing.
- Alternate failures on two separate copies A/B of the same item type. A's failures must not accumulate on B. Alternate players on A and verify they share A's failure count.
- On a valid non-free item, interleave a successful discount between failures and confirm it does not reset the count. Success must not apply a rejection effect or increase failures.
- Check thresholds 1 and 5. At 1, the first failure retains stock behavior; at 5, the first four failures must not arm the native destruction effect.
- Change the threshold after a verdict but before the delayed rejection finishes. The current verdict keeps its original decision; the next verdict uses the new setting and retained failure count.
- Repeat in a host-only mixed lobby. Both screens must see ordinary rejection animations below threshold with no delayed explosion, and the same native exploding effect at threshold. Test late join before and after threshold.
- Destroy/replace or reroll an item and confirm the replacement starts with zero failures. An interrupted scan before a verdict must not add a failure. Unrelated damage/destruction is not suppressed.
