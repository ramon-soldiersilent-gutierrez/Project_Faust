# Project Faust

Project Faust is a prototype Diablo-like Action RPG created to explore a fundamental shift in game design: **Using AI as a core, player-facing game feature**, rather than a behind-the-scenes tool for asset generation, code writing, or level geometry.

In Project Faust, the AI is the "Demon in the Forge".

## The Core Mechanic: The Faustian Bargain

Instead of relying on random number generators (RNG) and static loot tables to acquire gear, players interact directly with an ancient, dark intelligence (an LLM). 

The player declares a **Wish** and explicitly sets their **Greed Level** (0-100). The AI processes these inputs and generates a `ContractModel`—a custom, data-driven ability that grants the player immense power, but maliciously twists the wish by attaching a "Monkey's Paw" curse proportional to their greed.

If you push the Greed slider to 100, the AI might grant you a god-tier screen-clearing spell, but curse you so that every time you cast it, you are rooted in place and take 10% of your max health in damage. You have to play around the exact demonic trap the AI designed for *you*.

## Technical Architecture

This project is a vertical slice designed specifically for a 7-hour hackathon, running on Unity 6. The architecture proves that the LLM is not directly mutating code or hallucinating game mechanics. Instead, it navigates a highly strict, semantic registry:

1. **Path of Building Math:** Modifier logic uses a strict `(Base + Flat) * (1 + Increased) * More` arithmetic pipeline. The AI cannot break the game math.
2. **Data-Driven Composition:** Skills, Boons, and Curses are defined as Unity ScriptableObjects. The LLM only returns string IDs, and the Unity engine safely instantiates the corresponding `IHookInstances` and validates `GemTag` routing.
3. **Hard Fallbacks:** If the API times out, the `AIPipeline` deterministically generates a fallback contract, ensuring the loop never drops.

## Hackathon Execution (Parallel Agents)

Project Faust was built by 4 parallel AI agents operating in 4 isolated Git Worktrees to maximize throughput:

* **Worktree A:** Simulation Core (FakeECS runtime, bullet pooling, hit detection).
* **Worktree B:** Stat Pipeline & Hooks (Modifier calculation, hook injection).
* **Worktree C:** UI & Demo Harness (Contract Forge UI, F12 macro resets).
* **Worktree D:** AI Adapter & Fallbacks (Gemini client, JSON deserialization).

Shared interfaces ("Factory Rails") were scaffolded upfront to decouple dependencies. First-commit-wins.
