# PROJECT FAUST: ARCHITECTURAL INVARIANTS

**CRITICAL CONTEXT FOR AI AGENTS & DEVELOPERS:**  
We have 7 hours to build a vertical slice of a "Monkey's Paw" ARPG system for a hackathon. We are using Unity 6, C#, and the Gemini 2.0 Flash API. Stability, performance, and a deterministic demo path are our highest priorities.

Violating the following laws of physics will result in rejected code.

---

## 1. Architecture & Data Flow (The ARPG Pipeline)
Traditional per-entity behavior and deep inheritance hierarchies will fail in this domain. The runtime must be data-driven and system-oriented.

*   **Composition Over Inheritance:**
    *   ❌ **BANNED:** `class Fireball : Projectile` or `class Sword : Weapon`.
    *   ✅ **MANDATORY:** Use generic containers (`GenericProjectileSpawner`, `MeleeOverlapBox`). The identity of the attack is defined entirely by the `StatPackage` injected at runtime.

*   **Single Source of Truth for Stats (The ARPG Math Pipeline):**
    *   Scripts must NEVER directly mutate final stat values (e.g., `damage = 50` is illegal).
    *   All stats are resolved through a strict, single-source-of-truth pipeline.
    *   **Formula:** `FinalValue = (Base + Sum(Flat)) * (1 + Sum(Increased)) * Product(More)`.

*   **Event-Driven Side Effects (The Event Bus):**
    *   Boons and Curses are implemented as isolated Hooks that subscribe to global `CombatEvents` (`OnCast`, `OnHit`, `OnPlayerDamaged`).
    *   Hooks must be typed (`Action<CastInfo>`, `Action<HitInfo>`, etc.) and not parameterless.
    *   **Explicit Lifecycle:** When a new Faustian Contract is forged, the runtime MUST explicitly disable and unsubscribe all previous hooks from the `CombatEventBus` before enabling new ones. No lingering subscriptions.

*   **Tag-Based Routing (No Hardcoded Logic):**
    *   The AI outputs JSON containing `tags` and `hookIds`. The C# runtime interprets these via a `HookRegistry` (`Dictionary<string, HookFactory>`).
    *   ❌ **BANNED:** `if (weapon.tag == "Fire") { ApplyBurn(); }`.
    *   ✅ **MANDATORY:** `if (HookRegistry.TryGetValue(aiGeneratedId, out var factory)) { activeHooks.Add(factory.Create(ctx)); }`.
    *   Unknown IDs are dropped and reported to the `AI_OutputConsole`.

*   **Hook Identity & Ownership:**
    *   Hooks are instantiated as `IHookInstance` objects with explicit lifecycle methods:
        *   `Enable(CombatEventBus bus, PlayerContext ctx)`
        *   `Disable(CombatEventBus bus)`
    *   The runtime owns `List<IHookInstance> activeHooks` and is responsible for disabling and clearing this list on reforge and demo reset.

---

## 2. Runtime & Performance (FakeECS)
We cannot afford garbage collection spikes or PhysX instability during a live demo.

*   **No Runtime Instantiate/Destroy:**
    *   Use pre-allocated Object Pools (e.g., 200 Projectiles, 100 Enemies spawned at `Start()`).
    *   Toggle `gameObject.SetActive()`.
    *   If the pool is empty, silently drop the spawn and log a rate-limited warning to the in-game console.

*   **One Update Loop Per Domain:**
    *   A central `SimulationManager` ticks flat lists of data-only structs (e.g., `List<ProjectileBody>`, `List<EnemyBody>`).
    *   No parallel arrays/lists. Each entity is represented by a single struct containing `Transform`, velocity, radius, team, and active state.
    *   Prefabs (Projectiles/Enemies) must have NO `Update()` or `FixedUpdate()` methods.

*   **No Hot-Loop Allocations:**
    *   No LINQ, no string concatenation, no `foreach` over dynamic lists inside the main simulation loop.
    *   Allocations are permitted in UI and debug-only paths.

---

## 3. AI Constraints & Integration (The Gemini Layer)
The LLM is a chaotic actor. Its output must be constrained to prevent engine instability.

*   **Strict Output Schema (Raw JSON Only):**
    The model must emit raw JSON matching this exact shape:
    ```json
    {
      "itemName": "string",
      "skillPref": "Kinetic_Projectile | Kinetic_Sweep",
      "tags": ["Fire | Projectile | Melee | Speed | Self"],
      "boons": [{ "id": "Boon_DamageSpike | Boon_MachineGun | Boon_Multicast | Boon_Vampiric", "magnitude": 0.0 }],
      "curses": [{ "id": "Curse_TeleportOnHit | Curse_GlassCannon | Curse_SelfDamage | Curse_Rooted", "magnitude": 0.0 }]
    }
    ```
    *   Any value outside these enums MUST trigger the fallback compiler.

*   **Per-Stat Safety Clamps (Protect PhysX & Simulation):**
    All AI-generated magnitudes must be clamped by stat type before application:
    | Stat               | Min  | Max |
    |--------------------|------|-----|
    | MoveSpeed          | 0    | 15  |
    | AttackSpeedMult    | 0.1  | 10  |
    | ProjectileSpeed    | 1    | 50  |
    | DamageFlat         | 0    | 500 |
    | TeleportRadius     | 0    | 7   |

*   **Async UI Lock:**
    *   The "FORGE" button must disable immediately on click and only re-enable when the API returns or times out. No double-forging.

*   **No Silent Failures (Fallback Compiler):**
    *   Any API error, timeout, or JSON parse failure MUST trigger `FallbackCompiler.GenerateContract()`.
    *   Failures must be reported in the `AI_OutputConsole`. The demo must remain playable if the network fails.

---

## 4. Gameplay & Demo Stability (The "Wow" Path)
Hackathon judging is chaotic. The demo path must be repeatable and visually striking.

*   **Scope Limit:**
    *   Exactly **2 Skills**: `Kinetic_Projectile`, `Kinetic_Sweep`.
    *   Exactly **4 Boons** and **4 Curses** as defined in the schema above.

*   **Paired Modifiers:**
    *   Every boon requires a paired curse. No pure upside. Greed tier controls severity, not presence.

*   **Deterministic Greed Tiers:**
    *   `0–30`: 1 boon (mild) + 1 curse (mild)
    *   `31–70`: 1 boon (stronger) + 1 curse (spicy)
    *   `71–100`: 1 boon (max) + 1 signature curse (Glass Cannon / One-Punch class effects)

*   **Demo Reset (`DemoMode_Macro` bound to F12):**
    Pressing F12 must instantly:
    *   Wipe all enemy and projectile lists
    *   Restore player health and base stats
    *   Disable and clear all active hooks and `CombatEvents` subscriptions
    *   Reset `StatPackage` to base
    *   Reset `Time.timeScale = 1f` and clear any hit-stop timers
    *   Empty UI fields and reset the camera

*   **Explainable AI:**
    *   The `AI_OutputConsole` must visibly display tags, selected hook IDs, and final JSON.
    *   Dropped/unknown IDs and fallback usage must be printed to the console.

---