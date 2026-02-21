# agent_d.md — AI Adapter & Fallbacks (Worktree D)

## Mission
Own the **Gemini integration** and **contract generation pipeline**. The system must always return a valid `ContractModel`, even if the network dies.

## Scope (You Own)
- `GeminiClient` (UnityWebRequest + timeout)
- `ContractParser` (strict schema validation)
- `FallbackCompiler.GenerateContract(wish, greedTier)`
- Optional Ollama adapter (non-blocking)

## Interfaces You Must Respect
- `ContractModel` DTO (do not mutate fields outside schema)
- `ILogSink` (write AI output to console)
- `ContractRuntime.ApplyContract(...)` will consume your output

## Hard Invariants (Specific to You)
- ❌ No prose output from the LLM (raw JSON only)
- ❌ No silent failures
- ✅ Enforce enum whitelist for skills/boons/curses
- ✅ Clamp all numeric values before returning
- ✅ Hard timeout (5–8s max) → fallback immediately

## Definition of Done
- `ContractService.Generate(wish, greed)` always returns a valid ContractModel.
- API failure or JSON parse error triggers fallback.
- Raw JSON and fallback usage are printed to AI_OutputConsole.