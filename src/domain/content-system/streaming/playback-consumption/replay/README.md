# replay (SCAFFOLD — pending implementation)

## Purpose

The `replay` leaf owns **viewer-initiated replay lifecycle** of a stored archive — the viewer-side counterpart to `live-streaming/archive`. Tracks replay-request, replay-initiation, replay-progress, and replay-termination.

## Owns (planned)

- Replay identity, archive ref, viewer/session ref, position, status (Requested / Active / Paused / Completed / Abandoned).
- Request / start / pause / resume / complete / abandon transitions.

## Does not own

- The archive itself — owned by `live-streaming/archive`.
- The playback telemetry (bitrate, drop) during replay — owned by `delivery-governance/observability`.
- Viewer identity — identity system.

## §DF-06 note

Per the recording-split partition verdict (§claude/new-rules/20260420-091939-audits.md), NO current `Recording*` events migrate here. All legacy events are broadcast-side and go to `archive`. This leaf is scaffolded empty until a viewer-replay feature is implemented.

## Status

SCAFFOLD only in P2.6.CS.6. All 7 mandatory artifact subfolders exist with `.gitkeep` placeholders.
