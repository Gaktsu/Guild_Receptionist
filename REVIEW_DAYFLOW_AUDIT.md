# Senior Unity Gameplay Review — Day Flow / State-driven MVP

Scope reviewed:
- `DayFlowOrchestrator`, `DaySystem`, `GameSession`, `GameBootstrapper`
- UI panels (`InfoPanel`, `QuestPanel`, `ResultPanel`)
- `SaveSystem` / `SaveGameData`
- `QuestSystem` / `EventSystem`

## High-risk findings

1. **Loaded AP value is always overwritten at day start (save/load side-effect).**
   - `LoadGame` restores `CurrentAP` from save (`ActionPointSystem.SetCurrent(saveData.CurrentAP)`), but orchestrator immediately executes `DayStart` logic and calls `_apSystem.StartDay()`, resetting AP to max every boot. This makes persisted AP effectively non-authoritative on load.
   - References: `GameSession.LoadGame` and `DayFlowOrchestrator.HandleStateChanged(DayStart)`.

2. **World-state save can be stale for one full day when app exits mid-day.**
   - DayStart applies event-driven world delta and Resolution applies quest deltas, but no save is triggered at either point.
   - Save only happens in `NextDay()` (DayEnd), so crash/quit before DayEnd loses world-state mutations from the current day.
   - References: `DayFlowOrchestrator.HandleStateChanged` (`DayStart`, `ResolutionPhase`, `DayEnd`) and `GameSession.NextDay`.

3. **Event progression can advance twice per in-game day.**
   - `TryTriggerOrAdvance` is called in DayStart and again in Resolution.
   - This allows phase changes from both time-based checks and success/failure checks inside one day tick, which is often a lifecycle smell unless explicitly intended.
   - Reference: `DayFlowOrchestrator.HandleStateChanged` in `DayStart` and `ResolutionPhase`.

## Medium-risk findings

4. **`ForceNewGame()` bypasses day-flow rebootstrap.**
   - `GameBootstrapper.ForceNewGame()` calls `Session.NewGame()` but does not invoke orchestrator start hooks (which regenerate infos, event daily delta, etc.).
   - Depending on current state, UI/data may remain stale until the next state transition.
   - References: `GameBootstrapper.ForceNewGame`, `GameSession.NewGame`, `DayFlowOrchestrator.Init`.

5. **Result panel initialization is inconsistent with other panels.**
   - `InfoPanel` and `QuestPanel` are initialized in bootstrapper before orchestrator init.
   - `ResultPanel` self-initializes in `Start()` via singleton lookup. This split pattern can cause race/coupling issues across scenes/prefab variants.
   - References: `GameBootstrapper.Awake`, `ResultPanel.Start`.

6. **Singleton cleanup omission (`GameBootstrapper.Instance` never cleared).**
   - There is no `OnDestroy` that nulls static `Instance`; if destroyed unexpectedly, stale static refs can survive and break late lookups.
   - Reference: `GameBootstrapper` (no destroy cleanup).

## Low-risk observations

7. **State entry side-effects are all in `HandleStateChanged`, including nested transition from DayStart→InfoPhase.**
   - This is workable but tightly couples transition and side-effects; testing and deterministic replay become harder without explicit enter/exit command objects.
   - References: `DayFlowOrchestrator.HandleStateChanged`, `_daySystem.TrySetState(DayState.InfoPhase)`.

8. **Quest panel has an unused toggle group field (`_infoToggleGroup`).**
   - Not a correctness issue, but indicates incomplete UX contract (single-select vs multi-select).
   - Reference: `QuestPanel`.

## Recommended architectural adjustments (MVP-friendly)

1. **Separate "day bootstrap" from "state transition"**
   - Add explicit `BeginDay()` orchestration command that does event tick + AP reset + info generation + optional save.
   - Let state machine only represent phase, not perform all lifecycle mutations in event handlers.

2. **Define save policy by lifecycle milestone**
   - Save after any world mutation that must survive crash (`ApplyWorldDelta`, quest resolution, event progression).
   - Or maintain a dirty flag and autosave on app pause/quit.

3. **Make load semantics explicit**
   - If loading should always restart at fresh day, remove `CurrentAP` from save and document it.
   - If loading should resume mid-day, do **not** auto-run day-start reset on boot.

4. **Unify UI registration through bootstrapper/composition root**
   - Initialize `ResultPanel` through same root path as other panels, avoid singleton pull from `Start()`.

## Suggested targeted tests

- Load save with `CurrentAP=2` at Day 3, verify AP after boot is either 2 (resume model) or 5 (fresh-day model) by design.
- Crash simulation after Resolution and before DayEnd, verify world state and event phase persistence on relaunch.
- One in-game day with 3 successes: verify event phase transition count is exactly one expected step.
- ForceNewGame during non-DayStart state: verify all panels and generated info reset consistently.
