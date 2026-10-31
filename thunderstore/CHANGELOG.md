# Changelog

## 0.1.1

- Add BreakAfterFailures (1-5, default 1) to REPOConfig.
- Count failed discount verdicts per physical item, shared by players. Successful discounts do not reset accumulated failures.
- Preserve native rejection animations below the threshold while withholding the exploding-item effect. At the threshold, use native destruction and synchronization unchanged.
- Deduplicate verdict callbacks and keep in-progress rejection decisions stable when settings change.

## 0.1.0

- Name the mod ShopkeeperDiscounts and the project REPOShopkeeperDiscounts.

- Default to a 70% positive verdict chance and five shared attempts per shop.

- Configure the vanilla shopkeeper positive verdict chance from 0 to 100 percent.
- Configure lobby-wide attempts per shop from 0 to 10; zero disables attempts.
- Expose both bounded integer settings through REPOConfig on the host.
- Preserve spent attempts across native state synchronization, including allowances above three.
- Keep native discount/failure reactions, item prices, effects, and RPCs for unmodded clients.
