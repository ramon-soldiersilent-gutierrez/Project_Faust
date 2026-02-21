# agent_b.md — Stat Pipeline & Hooks (Worktree B)

## Mission
Own the **ARPG math pipeline** and **Boon/Curse hook system**. Deliver predictable stat resolution and clean hook lifecycle.

## Scope (You Own)
- `StatPackage` + `ModifierCalculator`
- `AbilityContext` builder
- `HookRegistry` (ID → Hook factory)
- `HookLifecycleManager`
- All 8 hooks:
  - Boons: DamageSpike, MachineGun, Multicast, Vampiric
  - Curses: TeleportOnHit, GlassCannon, SelfDamage, Rooted

## Interfaces You Must Respect
- `CombatEventBus` (typed events only)
- `SimulationAPI.SpawnProjectile(...)` (for Multicast)
- `PlayerContext` (health, speed, transform, scale)
- `ContractRuntime.ApplyContract(...)` will call into your lifecycle manager

## Hard Invariants (Specific to You)
- ❌ No direct mutation of final stats (no `damage = 50`)
- ✅ All stats resolved via `(Base + Flat) * (1 + Increased) * More`
- ❌ No hardcoded tag logic (`if (tag == Fire)`)
- ✅ Hooks implement `IHookInstance` with `Enable/Disable`
- ✅ Hooks must be unsubscribed on reforge and demo reset

## Definition of Done
- Applying a hardcoded Contract visibly alters gameplay.
- Hooks enable/disable cleanly on reforge.
- No duplicate event firing, no lingering subscriptions.