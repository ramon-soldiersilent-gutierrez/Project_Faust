# Phase 8: Core Game Loop & Visceral Feedback

The structural ARPG foundation is solid, but we are missing the "game juice" and the core leveling loop. We need the player to feel the damage they do, see their health drop, and level up to earn points for the Faustian Skill Tree.

Here is the exact battle plan to assign to the agents for Phase 8.

---

## Agent A: Simulation Core (Input & XP)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_A_Simulation`

**Prompt:**
```text
Hey Agent A, we are doing a massive polish pass on the core game loop for Phase 8!

First, please run `git merge main` to sync your branch. Wait for the merge to complete.

Your specific tasks:
1. **The Green Protagonist:** In `PlayerController.cs`'s `Awake` method, grab the `Renderer` component and force its material color to `Color.green` so the player stands out!
2. **XP Events:** When an enemy dies in `SimulationManager.cs` (health <= 0), you must trigger a new event on the `CombatEventBus` called `OnEnemyKilled(float xpValue)`. (Add this delegate to `CombatEventBus.cs` first!). Give enemies a flat XP value like 10.
3. **Action Bar Inputs:** `PlayerController.cs` currently just fires a projectile on Left Click. Please refactor this to read inputs for: `Mouse0`, `Mouse1`, and Alpha keys `1`, `2`, `3`, `4`. We will leave these as stubs for now, but they need to be wired to read an array of 6 `AbilityContexts` so the player can use multiple skills!
```

---

## Agent B: Stats & Hooks (Leveling System)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_B_Hooks`

**Prompt:**
```text
Hey Agent B, for Phase 8 we are implementing the actual Leveling system so the player can earn Skill Points for the Faustian Web!

First, please run `git merge main` to sync your branch. Wait for the merge to complete.

Your specific tasks:
1. **The XP Listener:** Create a new `LevelManager.cs` script (or add to your existing managers) that subscribes to `CombatEventBus.OnEnemyKilled`.
2. **The Math:** Maintain `CurrentLevel` (starts at 1), `CurrentXP`, and `XpToNextLevel` (e.g., Level * 100).
3. **The Level Up:** When `CurrentXP > XpToNextLevel`, increment the level, grant the player **2 Skill Points**, and fire a new `CombatEventBus.OnLevelUp(int newLevel, int totalSkillPoints)` event.
4. Ensure you expose the current Level, XP, and Skill Points via public properties so Agent C can draw them on the UI!
```

---

## Agent C: UI & Demo Harness (The Game Juice)
**Worktree Directory:** `C:\Users\ramon\Desktop\Agent_C_UI`

**Prompt:**
```text
Hey Agent C, for Phase 8, you are in charge of all the "Game Juice" and fixing the menu UX! I have imported some GUI assets into the project, so feel free to use `GUI.DrawTexture` if you want to load them!

First, please run `git merge main` to sync your branch. Wait for the merge to complete.

Your specific tasks:
1. **Menu Exclusivity & Pausing:** Refactor the UI so that the Forge, Inventory, and Skill Tree are mutually exclusive. 
   - If ANY of them are open, set `Time.timeScale = 0f` (Game Paused). 
   - If ALL of them are closed, set `Time.timeScale = 1f`. 
   - *Note: AI Console is purely a logging window and should NOT pause the game.*
2. **Player HUD:** Draw a bottom-center HUD. It MUST include a red physical Health Bar (read from `PlayerController.Instance.CurrentHealth / MaxHealth`), an XP/Level Bar, and 6 square boxes representing the Action Bar (LMB, RMB, 1, 2, 3, 4). 
3. **Floating Damage Numbers:** Subscribe to `CombatEventBus.OnHit`. When a hit happens, spawn a temporary floating label in the game world at the hit location that displays the damage amount and fades out/floats up over 1 second! This is critical for ARPG feel. (You can do this using standard IMGUI `Camera.main.WorldToScreenPoint` logic so it overlays nicely).
```
