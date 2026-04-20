# streaming / playback-consumption

## Purpose

Groups **viewer-side consumption** concerns — sessions, progress tracking, and replay of archived broadcasts. Distinct from stream identity and distinct from broadcast-side concerns.

## Leaf domains

- `session/` — per-session attachment of a viewer to a stream, with window enforcement (Opened / Active / Suspended / Closed / Failed / Expired). Moved from `stream-core/stream-session`. Aggregate class `StreamSessionAggregate` retains its name pending CS.13 Band-F rename.
- `progress/` — (SCAFFOLD pending CS.7) viewer playback progress.
- `replay/` — (SCAFFOLD produced by CS.6) replay lifecycle of stored broadcast archives, distinct from archive's broadcast-side persistence.

## Boundary notes

- Session does NOT own viewer identity — session references an opaque viewer/token binding. Identity is owned by the identity context.
- Replay ≠ archive. Archive is broadcast-side (the stored output exists). Replay is viewer-side (a viewer has initiated a re-watch of the archive).
