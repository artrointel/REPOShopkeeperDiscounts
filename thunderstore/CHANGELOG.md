# Changelog

## 0.1.0

- Name the mod ShopkeeperDiscounts and the project REPOShopkeeperDiscounts.

- Default to a 70% positive verdict chance and five shared attempts per shop.

- Configure the vanilla shopkeeper positive verdict chance from 0 to 100 percent.
- Configure lobby-wide attempts per shop from 0 to 10; zero disables attempts.
- Expose both bounded integer settings through REPOConfig on the host.
- Preserve spent attempts across native state synchronization, including allowances above three.
- Keep native discount/failure reactions, item prices, effects, and RPCs for unmodded clients.
