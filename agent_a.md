# agent_a.md — Simulation Core (Worktree A)

## Mission
Own the **runtime simulation loop** (FakeECS). Deliver a playable world where the player moves, enemies chase, projectiles spawn, and hits register with no GC spikes and no per-entity Update methods.

## Scope (You Own)
- `SimulationManager` (central Update loop)
- Entity structs: `ProjectileBody`, `EnemyBody`
- Object pooling integration (projectiles + enemies)
- Distance-based hit detection
- `SimulationAPI.SpawnProjectile(...)` implementation

## Interfaces You Must Respect
- `CombatEventBus` (raise `OnCast`, `OnHit`)
- `AbilityContext` (read projectile speed/damage only)
- `DemoAPI.ResetAll()` must call your reset hooks
- No changes to Stat math, hooks, or UI contracts

## Hard Invariants (Specific to You)
- ❌ No per-entity `Update()` / `FixedUpdate()` on prefabs  
- ❌ No runtime `Instantiate` / `Destroy`  
- ❌ No LINQ or allocations in hot loops  
- ✅ One Update loop ticks `List<ProjectileBody>` and `List<EnemyBody>`  
- ✅ Prefer `Vector3.Distance` for hit detection (BoxCast only if strictly needed)

## Definition of Done
- Left-click spawns projectile.
- Projectile hits cube and despawns it.
- Cubes chase player.
- No GC spikes or NullReferenceExceptions.