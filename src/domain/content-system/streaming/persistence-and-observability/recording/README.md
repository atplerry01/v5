# recording

## Purpose

The `recording` leaf owns stream-native durable recording — the act of capturing a running stream to a persistent output, with a started → completed → finalized → archived happy path and a failure branch.

## Aggregate root

- `RecordingAggregate`

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

- `Start` creates the recording in `Started`.
- `Complete` is valid only from `Started`; rejected after failure.
- `Fail` is rejected from terminal states (`Failed`, `Finalized`, `Archived`).
- `Finalize` is valid only from `Completed`; rejects already-finalized.
- `Archive` is valid only from `Finalized`; rejects otherwise.
- Archived is terminal.
- `StreamRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedRecording`).

## Owns

- Recording identity, stream binding, optional session binding, output reference, failure reason, status, timestamps.
- Start / complete / fail / finalize / archive transitions.

## References

- `StreamRef` — the stream being recorded.
- `StreamSessionRef` (optional) — the specific session being recorded, if scoped to a session.
- `RecordingOutputRef` — produced durable output, attached on completion.

## Does not own

- The encoder / writer pipeline — infrastructure.
- The storage backend for the output bytes — infrastructure.
- Post-recording publication (e.g. creating a `media/content-artifact/asset` from a recording) — that would be an engine-level orchestration, not a domain concern here.
