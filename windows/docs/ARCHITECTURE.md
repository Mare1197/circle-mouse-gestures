# Windows application architecture

Circle Mouse is split so Windows-only input code never leaks into deterministic rule and configuration logic.

## Milestone 1

- **Core** owns versioned configuration, profiles, mappings, action definitions, and deterministic resolution.
- **Windows** owns the `WH_MOUSE_LL` hook, foreground-window discovery, and `SendInput` action execution.
- **App** owns lifetime, the tray, and a bounded single-reader input channel. The hook callback only attempts a non-blocking enqueue; profile/context lookup and action execution happen on the channel consumer.
- **Core.Tests** tests rule selection without requiring Windows hooks.

The vertical slice captures XButton1 globally and consumes it only after the bounded queue accepts it. The worker resolves the foreground executable against application profiles and invokes the shared action executor. The generated default maps XButton1 to Ctrl+Shift+T. Configuration lives under `%LOCALAPPDATA%\CircleMouse\config.json`; normal operation uses `asInvoker` and per-monitor-v2 DPI awareness.

## Target modules and phased plan

1. Add diagnostics, an emergency keyboard bypass, reload, logging, and full button/wheel actions; preserve the bounded event queue and define per-action failure behavior.
2. Extend resolution with named layers, window rules, priorities, and an explainable resolution trace. Resolution order is context rule, window rule, matching application, active layer, then global fallback; specificity wins before numeric priority, and declaration order breaks remaining ties.
3. Add a timing state machine for tap, hold, long-hold, double-click, chords, wheel, and movement.
4. Implement a DPI-aware, no-activation pie overlay whose slices call the same action executor.
5. Add directional gesture recognition and a transparent trail overlay, also calling the shared executor.
6. Add asynchronous UI Automation context snapshots with explicit override and graceful generic-context fallback.
7. Add macros, process/window/media actions, cancellation, and execution safeguards.
8. Build a WinUI 3 settings and diagnostics client over the same JSON model.
9. Profile latency and idle resources, fuzz migrations/state machines, and ship an MSIX/installer with startup integration.

Pie menu and gesture definitions remain action types in Core; their selected slices/gestures resolve to ordinary action definitions. No subsystem gets a parallel execution path.
