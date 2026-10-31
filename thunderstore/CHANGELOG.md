# Changelog

## 0.1.1

Initial public release.

- Configure discount success chance from 0 to 100% (default 70%).
- Configure shared attempts per shop from 0 to 10 (default 5); zero disables attempts.
- Configure failures before an item breaks from 1 to 5 (default 1).
- Track cumulative failures separately for each physical item, shared by all players. Successful discounts do not reset failures.
- Change settings during play through REPOConfig without restarting.
- Preserve spent attempts when settings change, native state synchronization, and in-progress rejection decisions.
- Use native shopkeeper animations, sounds, discounts, item prices, and destruction effects. Only the host needs the mod and its dependencies; joining players can remain vanilla.
