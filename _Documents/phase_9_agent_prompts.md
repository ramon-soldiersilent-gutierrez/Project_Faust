# Phase 9: Demo Polish & Overhaul

This is the final stretch. The bones are there, but we need the systems to actually talk to each other so the player experiences the full ARPG loop (Shoot -> Kill -> Level -> Allocate Tree -> Check Stats -> Repeat).

Here are the specific assignments for the agents to resolve the feedback.

---

## Agent A: Simulation Core (Combat Interactions)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_A_Simulation`

**Prompt:**
```text
Hey Agent A! The combat simulation is missing a few critical interactions. Please implement the following:

1. **Enemy Collision Damage:** In `SimulationManager.cs` `Update()` loop, check if any enemy is within `HitRadius` of the `PlayerTransform.position`. If so, inflict 5 damage to the player (`PlayerController.Instance.TakeDamage(5)`) and bounce the enemy back slightly so they don't infinitely trigger damage every frame.
2. **Player Hit Visual:** In `PlayerController.cs`, when `TakeDamage` is called, flash the `renderer.material.color` to Red for 0.1s using a Coroutine, then revert to Green.
3. **Skill Mapping:** Wire `PlayerController.cs` inputs. 
   - Left-Click (Mouse0) MUST execute the `Kinetic_Projectile` skill.
   - Right-Click (Mouse1) MUST execute the `Kinetic_Sweep` skill.
4. **Enemy Variety (Optional Bonus):** If you can, make 20% of spawned enemies Orange instead of Red. These orange enemies should stop moving at a distance of 10 units and fire a slow, forward-moving projectile at the player every 2 seconds.
```

---

## Agent B: Stats & Hooks (Tree Integration)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_B_Hooks`

**Prompt:**
```text
Hey Agent B! We need the Skill Tree to actually affect stats.

1. **Tree Allocation State:** Look at `SkillTreeModel.cs` and `HookLifecycleManager.cs`. `HookLifecycleManager` needs to maintain a `HashSet<string> AllocatedNodeIDs`.
2. **Apply Nodes:** When building the final stats and injecting hooks, you must iterate over BOTH the equipped `ContractModels` AND the currently allocated `SkillTreeNode` objects from the UI, aggregating their `DamageDelta`, `SpeedDelta`, and granting their Boons/Curses.
3. Provide a public method `public void ToggleNodeAllocation(SkillTreeNode node)` that checks if `LevelManager.Instance.SpendSkillPoints(1)` returns true. If so, add it to the allocated list and trigger a re-calculation of stats!
```

---

## Agent C: UI & Demo Harness (The Game Feel)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_C_UI`

**Prompt:**
```text
Hey Agent C! The UI needs a massive overhaul to match modern ARPG standards (Diablo/PoE).

1. **The Missing XP Bar:** In `PlayerHUD.cs`, you forgot to draw the XP Bar! Please draw a thin yellow bar underneath or above the Action Bar that maps to `LevelManager.Instance.CurrentXP / LevelManager.Instance.XpToNextLevel`.
2. **Character Sheet Redesign:** Rename the concept of "Inventory" to "Character Sheet". `ContractUI.cs` should draw a wide window. The Left Side should show the player's core stats (Current HP, Damage Multiplier, Speed Multiplier, Active Boons). The Right Side should show the 3 Equipped Item Slots (Weapon, Armor, Accessory).
3. **Interactive Skill Tree:** Update `SkillTreeUI.cs`:
   - Display `Available Skill Points: X` at the top (read from `LevelManager.Instance`).
   - If a node is clicked, call `HookLifecycleManager.Instance.ToggleNodeAllocation(node)`.
   - Change the color of Allocated nodes to Gold so the player knows they own them!
   - Add a "Reset Tree" button that clears all allocated nodes and refunds points.
```
