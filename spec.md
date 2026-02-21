# PROJECT FAUST: HACKATHON EXECUTION PLAN

This file defines both the **Definition of Done checklist** and the **parallel work breakdown** for running Project Faust as a small, high-throughput software factory during the hackathon.  
We are running **4 agents in parallel** with **first-commit-wins**. Rebasing is expected. Stability, deterministic demo flow, and integration velocity take priority over elegance.

---

## GLOBAL EXECUTION RULES

- **First-Commit-Wins:** If two agents touch the same file, the first merged commit wins. Others rebase.
- **Interfaces First:** Shared interfaces and data contracts must be defined before feature work begins.
- **Single Scene Owner:** Only one agent (UI owner) may modify Unity scenes. Others work in scripts/prefabs only.
- **Rails Before Lanes:** Core interfaces (“Factory Rails”) must land before parallel feature work begins.
- **Demo-First:** Every slice must end in a runnable, demo-progressive state.

---

## FACTORY RAILS (LOCK THESE FIRST)

These types are the shared contracts all worktrees plug into. They should be committed as stubs immediately to minimize merge conflicts.

- `CombatEventBus`  
  - Typed events: `OnCast(CastInfo)`, `OnHit(HitInfo)`, `OnPlayerDamaged(float)`
- `StatPackage` + `ModifierCalculator`
- `AbilityContext`
- `ContractModel` (parsed AI JSON DTO)
- `IHookInstance` + `HookRegistry` + `HookLifecycleManager`
- Glue APIs:
  - `ContractRuntime.ApplyContract(ContractModel model)`
  - `SimulationAPI.SpawnProjectile(in AbilityContext ctx, Vector3 pos, Vector3 dir)`
  - `DemoAPI.ResetAll()`

> **Rule:** All agents must code against these interfaces. Implementation details live behind them.

---

## PARALLEL WORK BREAKDOWN (4 AGENTS)

### AGENT A — SIMULATION CORE (Worktree A)
**Deliverable:** The world runs. Player moves. Cubes chase. Projectiles hit. No per-entity Update.

**Owns:**
- `SimulationManager` (FakeECS tick loop)
- Entity structs: `ProjectileBody`, `EnemyBody`
- Object pooling integration (spawn/despawn)
- Hit detection (distance-based preferred)

**Consumes:**
- `CombatEventBus` (raise `OnCast`, `OnHit`)
- `AbilityContext` (read projectile speed/damage)
- `SimulationAPI.SpawnProjectile(...)` (authoritative implementation)

**Definition of Done:**
- Left-click spawns projectile.
- Projectile hits cube.
- Cube despawns.
- No GC spikes, no NullReferenceExceptions.

---

### AGENT B — STAT PIPELINE & HOOKS (Worktree B)
**Deliverable:** Boons/curses modify gameplay via hooks without memory leaks.

**Owns:**
- `StatPackage`, `ModifierCalculator`
- `HookLifecycleManager`
- `HookRegistry` (ID → Hook factory)
- All 8 hooks (4 boons, 4 curses)

**Consumes:**
- `CombatEventBus` (subscribe/unsubscribe)
- `PlayerContext` (health, speed, transform, scale)
- `SimulationAPI.SpawnProjectile(...)` (for Multicast)

**Definition of Done:**
- Applying a hardcoded Contract visibly changes gameplay.
- Hooks enable/disable cleanly on reforge/reset.
- No duplicate event firing.

---

### AGENT C — CONTRACT UI & DEMO HARNESS (Worktree C)
**Deliverable:** Demo control is bulletproof. Judges can see AI output. F12 always resets.

**Owns:**
- `ContractUI` (wish input, greed slider, forge button)
- `AI_OutputConsole`
- `DemoMode_Macro` (F12 reset)
- Greed tier mapping logic

**Consumes:**
- `ContractRuntime.ApplyContract(...)`
- `DemoAPI.ResetAll()`
- `HookLifecycleManager.Reset()`
- `PlayerContext.Reset()`

**Definition of Done:**
- FORGE applies a contract (hardcoded first).
- AI output console prints structured output.
- F12 returns game to known-good state every time.

---

### AGENT D — AI ADAPTER & FALLBACKS (Worktree D)
**Deliverable:** Text → JSON → ContractModel always returns within timeout, even if API fails.

**Owns:**
- `GeminiClient` (UnityWebRequest + timeout)
- `ContractParser` (strict schema validation)
- `FallbackCompiler.GenerateContract(...)`
- Optional Ollama adapter (non-blocking)

**Consumes:**
- `ContractModel` DTO
- `ILogSink` interface for writing to `AI_OutputConsole`

**Definition of Done:**
- `ContractService.Generate(wish, greed)` always returns a valid ContractModel.
- API failures/timeouts fall back deterministically.
- Raw JSON is logged to output console.

---

## MERGE ORDER (ENFORCED)

1. **Factory Rails (interfaces + stubs)**
2. **Simulation Core (Agent A)**
3. **Stat Pipeline & Hooks (Agent B)**
4. **Contract UI & Demo Harness (Agent C)**
5. **AI Adapter & Fallbacks (Agent D)**

> **Rule:** Phase 4 (Skills) may not start until Phase 2 (Stat Pipeline & Event Bus) compiles and unit-tests pass.

---

## HACKATHON CHECKLIST (DEFINITION OF DONE)

### 🔴 PHASE 0: SCAFFOLDING (10:00 - 10:20)
- **Repo Initialization:** Unity 6 project created, Git initialized, `.gitignore` includes local API keys.
- **Invariants Set:** `INVARIANTS.md` pushed to main. Agents instructed to read it.

### 🟠 PHASE 1: CORE RUNTIME & FAKE ECS (Worktree A)
- **Object Pooler:** `ProjectilePool` (200) and `EnemyPool` (100) pre-allocated at `Start()`.
- **Simulation Manager:** Single `Update()` loop ticking `List<ProjectileBody>` and `List<EnemyBody>`. No parallel arrays.
- **Player Controller:** Basic capsule. WASD movement. Mouse aim.
- **Enemy Swarm:** Cubes move directly toward player. No NavMesh.
- **Hit Detection:** Prefer distance-based sphere checks. BoxCasts only if strictly necessary.

### 🟡 PHASE 2: STAT PIPELINE & EVENT BUS (Worktree B)
- **StatPackage Struct:** Base + Flat/Increased/More modifiers.
- **Modifier Pipeline:** `(Base + Sum(Flat)) * (1 + Sum(Increased)) * Product(More)`.
- **Combat Event Bus:** Typed delegates for `OnCast`, `OnHit`, `OnPlayerDamaged`.
- **Hook Lifecycle Manager:** Disable old hooks, clear list, enable new hooks on reforge/reset.

### 🟢 PHASE 3: AI ADAPTER & FALLBACKS (Worktree D)
- **Gemini Client:** Async requests with hard timeout (5–8s).
- **JSON Schema Enforcer:** Strict parsing into `ContractModel`.
- **Hardcoded Fallback:** Deterministic contract if API/parse fails.
- **Value Clamping:** Clamp all AI numeric values before engine use.
- *(Optional)* **Ollama Fallback:** Non-blocking, skip if unavailable.

### 🔵 PHASE 4: THE CONTRACT & SKILLS (Worktree C)
- **AbilityContext:** Built from `PlayerStats + StatPackage` at cast time.
- **Skill 1:** `Kinetic_Projectile`.
- **Skill 2:** `Kinetic_Sweep`.
- **Contract UI:** Wish input, Greed slider, FORGE button.
- **Async UI Lock:** FORGE disables during API call.
- **AI Output Console:** Shows tags + IDs + raw JSON.
- **Deterministic Greed Tiers:**
  - `0–30`: mild boon + mild curse  
  - `31–70`: stronger boon + spicier curse  
  - `71–100`: max boon + signature curse  

### 🟣 PHASE 5: THE MONKEY'S PAW CONTENT
- **Boons:** DamageSpike, MachineGun, Multicast, Vampiric.
- **Curses:** TeleportOnHit, GlassCannon, SelfDamage, Rooted.

### ⚫ PHASE 6: POLISH & DEMO PREP (16:00 - 17:00)
- **F12 Demo Reset:** Clears enemies/projectiles, resets player, disables hooks, resets stats, clears UI, resets camera.
- **Reset Time Scale:** `Time.timeScale = 1f`.
- **Hit Stop:** 0.05s time-scale freeze on big hits.
- **Screen Shake:** Camera shakes based on damage.
- **Record Wow Path:**  
  1) Low Greed (speed wish → minor curse)  
  2) High Greed (god damage → self-die curse)

### ⚪ PHASE 7: THE ARPG SHELL (Scope Expansion)
- **Tracking Camera (Agent A):** The camera rigidly frames the player rather than remaining static.
- **Inventory System (Agent C & B):** 
  - `ContractRuntime` must support multiple *Equipped Contracts* (e.g., Weapon, Armor, Ring).
  - UI for a basic inventory grid or slotting mechanism.
- **Procedural "Faustian" Skill Tree (Agent D & C):**
  - AI generates 1-3 new branching nodes on "Level Up".
  - Nodes grant specific Boons/Curses that act like Path of Exile Keystones.
  - Basic nodal UI to select and activate these procedurally generated branches.

---

## AGENTIC ACCELERATORS (OPTIONAL IF TIME PERMITS)

- **Interface Stub Generator:** Auto-generate Rails types with TODOs.
- **Hook Template Generator:** Generate 8 hook classes from a common template.
- **Schema + Prompt Pack:** Canonical Gemini system prompt + JSON schema + examples.
- **Demo Reset Codegen:** Generate `DemoAPI.ResetAll()` skeleton with ordered reset steps.

---

## FINAL NOTE

This project is a systems demo, not a content demo.  
If forced to cut scope, cut features — never cut **invariants, demo reset, or fallback paths**.