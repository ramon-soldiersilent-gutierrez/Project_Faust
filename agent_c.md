# agent_c.md — Contract UI & Demo Harness (Worktree C)

## Mission
Own the **player-facing demo loop**. Judges must be able to forge a contract, see AI output, and recover the demo instantly.

## Scope (You Own)
- `ContractUI` (Wish input, Greed slider, FORGE button)
- `AI_OutputConsole`
- `DemoMode_Macro` (F12 reset)
- Greed tier mapping (0–30 / 31–70 / 71–100)
- Scene wiring (you are the **only** scene editor)

## Interfaces You Must Respect
- `ContractRuntime.ApplyContract(ContractModel model)`
- `DemoAPI.ResetAll()`
- `HookLifecycleManager.Reset()`
- `PlayerContext.Reset()`
- `ILogSink` for AI output

## Hard Invariants (Specific to You)
- ❌ No gameplay logic in UI scripts  
- ✅ FORGE button must lock during async AI calls  
- ✅ AI_OutputConsole must show raw JSON + tags + hook IDs  
- ✅ F12 must return the game to a known-good state every time  

## Definition of Done
- FORGE applies a contract (hardcoded first, AI later).
- Output console prints structured JSON.
- F12 resets world, player, hooks, stats, camera, and timeScale.