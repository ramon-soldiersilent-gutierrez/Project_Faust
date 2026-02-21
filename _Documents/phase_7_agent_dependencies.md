# Phase 7: ARPG Cross-Agent Handshake & Combat Specs

To turn Project Faust into a true ARPG with Inventory and a Procedural Skill Tree, the 4 Agents must flawlessly interlock. This document defines the exact boundary lines, the damage math, and the 3 core skills they must support.

---

## 1. Cross-Agent Dependencies (The Handshake)

**Agent C (UI) & Agent D (AI)**
*   **The Bridge:** Agent C owns the visual Canvas. When the player levels up, Agent C sends the request to Agent D. Agent D hits Gemini and returns the `SkillTreeChunk` JSON. Agent C then dynamically instantiates a visual UI node graph based on `GridX` and `GridY`.
*   **The Inventory:** Agent C manages the visual slots (Weapon, Armor, Core Skill). When an item is equipped, Agent C tells Agent B to apply its stats.

**Agent B (Stats & Hooks)**
*   **The Engine:** Agent B is in charge of the mathematical state of the Player. 
*   **The Change:** `HookLifecycleManager` can no longer just hold *one* contract. It must now maintain a `List<ContractModel>` for Equipped Items AND a `List<SkillTreeNode>` for all activated nodes in the Skill Tree. 
*   **The Output:** When the player clicks to attack, Agent B recalculates the `AbilityContext` by stacking the modifiers from ALL items and ALL active tree nodes, and hands that final context to Agent A.

**Agent A (Simulation Core)**
*   **The Execution:** Agent A is completely blind to UI, Skill Trees, and Items. Its only job is to read the `AbilityContext` that Agent B hands it, and physically manifest it in the FakeECS world. If Agent B says the current skill does 5,000 damage with 15 projectiles, Agent A spawns 15 projectiles and subtracts 5,000 HP when they hit a cube.

---

## 2. The Universal Damage Math

All Agents must obey the Path of Building (PoB) mathematical pipeline. This ensures a minor $+5\%$ node and a massive $+100\%$ LLM Keystone calculate correctly.

**The Formula:**
`FinalStat = (BaseStat + Sum(FlatModifiers)) * (1.0 + Sum(IncreasedModifiers)) * Product(MoreModifiers)`

*   **Flat:** `AddFlat = 5` (+5 Base Damage)
*   **Increased/Decreased:** `AddIncreased = 0.5f` (+50% Increased Damage. These sum together additively before multiplying the base).
*   **More/Less:** `AddMore = 1.5f` (50% More Damage. These multiply sequentially, making them exponentially powerful for Curses/Keystones).

---

## 3. The 3 Core Archetype Skills

To prove the ARPG shell, Agent A must physically implement the behaviors for these 3 skills, and Agent B must define their base data in ScriptableObjects.

### 1. The Ranger: "Kinetic_Projectile" (Arrow Attack)
*   **Execution Shape:** `SkillShape.Projectile`
*   **Behavior (Agent A):** Spawns a physical entity from the `ProjectilePool` that travels in a straight line at `FinalProjectileSpeed` and hits the first enemy it touches.
*   **Tags:** `Projectile`, `Physical`

### 2. The Barbarian: "Kinetic_Sweep" (Melee Attack)
*   **Execution Shape:** `SkillShape.ForwardSweep`
*   **Behavior (Agent A):** Checks a semi-circle cone in front of the player using `FinalAreaRadius`. Any enemy caught in the cone takes immediate damage. No projectile spawned.
*   **Tags:** `Melee`, `AoE`, `Physical`

### 3. The Sorcerer: "Arcane_Aura" (Magic Attack)
*   **Execution Shape:** `SkillShape.TargetedAoE`
*   **Behavior (Agent A):** Drops a circular hazard at the exact location of the Mouse Cursor. After a `FinalCastTime` delay, all enemies within `FinalAreaRadius` take damage.
*   **Tags:** `Spell`, `AoE`, `Magic`
