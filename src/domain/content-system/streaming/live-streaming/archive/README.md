# archive (formerly `recording`)

## Purpose

The `archive` leaf owns **stream-native durable broadcast archive** — the act of capturing a running stream to a persistent output, with a started → completed → finalized → archived happy path and a failure branch. Moved from `streaming/persistence-and-observability/recording` in P2.6.CS.6 per §CD-08.

Per §DF-06 closure, **all legacy `Recording*` events partition here** (all events are broadcast-side; none are viewer-initiated). The companion `playback-consumption/replay` leaf is a SCAFFOLD only — no events migrate there.

## Aggregate root

- `RecordingAggregate` — class name RETAINED per CS.5 execution discipline (§claude/new-rules/20260420-091636-guards.md). CS.13 Band-F may rename to `ArchiveAggregate`.

## Key value objects

- `RecordingId`
- `StreamRef`
- `StreamSessionRef` (optional)
- `RecordingOutputRef`
- `RecordingFailureReason`
- `RecordingStatus` (Started / Completed / Failed / Finalized / Archived)

## Key events

- `RecordingStartedEvent`
- `RecordingCompletedEvent`
- `RecordingFailedEvent`
- `RecordingFinalizedEvent`
- `RecordingArchivedEvent`

## Invariants and lifecycle rules

- `Start` creates the archive in `Started`.
- `Complete` is valid only from `Started`; rejected after failure.
- `Fail` is rejected from terminal states.
- `Finalize` is valid only from `Completed`; rejects already-finalized.
- `Archive` is valid only from `Finalized`; rejects otherwise.
- Archived is terminal.
- `StreamRef` must not be `Guid.Empty`.

## Owns

- Archive identity (formerly recording identity), stream binding, optional session binding, output reference, failure reason, status, timestamps.
- Start / complete / fail / finalize / archive transitions.

## Does not own

- The encoder / writer pipeline — infrastructure.
- The storage backend for the output bytes — infrastructure.
- Viewer-initiated replay — owned by `playback-consumption/replay` (SCAFFOLD).
- Post-archive publication (e.g. creating a `media/core-object/asset` from an archive) — engine-level orchestration.

## Boundary notes

- `archive` is BROADCAST-SIDE truth: the archive exists because a broadcast was recorded and stored.
- `replay` is VIEWER-SIDE truth: a viewer initiated a re-watch of a stored archive. These are distinct lifecycles per §CD-08.
