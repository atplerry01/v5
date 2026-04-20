# processing

## Purpose

The `processing` leaf owns the transaction truth of a media processing job — transcoding, thumbnail generation, normalisation, packaging, or any other typed processing kind. It carries the job through requested → running → terminal outcome.

## Aggregate root

- `MediaProcessingAggregate`

## Key value objects

- `MediaProcessingJobId`
- `MediaProcessingKind`
- `MediaProcessingInputRef`
- `MediaProcessingOutputRef`
- `MediaProcessingStatus` (Requested / Running / Completed / Failed / Cancelled)
- `MediaProcessingFailureReason`

## Key events

- `MediaProcessingRequestedEvent`
- `MediaProcessingStartedEvent`
- `MediaProcessingCompletedEvent`
- `MediaProcessingFailedEvent`
- `MediaProcessingCancelledEvent`

## Invariants and lifecycle rules

- Linear ordered happy path: `Requested → Running → Completed`.
- `Start` is valid only from `Requested`; `Complete` is valid only from `Running`.
- `Fail` and `Cancel` are valid from any non-terminal state.
- Terminal states reject all further transitions.

## Owns

- Processing job identity, kind, input/output refs, status, failure reason, timeline.
- Request / start / complete / fail / cancel transitions.

## References

- `MediaProcessingInputRef` — opaque input (typically a `MediaFileRef`-shaped handle).
- `MediaProcessingOutputRef` — opaque output.

## Does not own

- The transcoder / packager / thumbnail engine — infrastructure.
- Resource allocation and queue prioritisation — orchestration.
- The asset or file being processed — owned by their respective leaves.

## Notes

- `MediaProcessingKind` encodes the variety of processing (transcode, thumbnail, normalise, package). The aggregate treats all kinds uniformly — only the kind value differs. Parallel in shape to `document/lifecycle/processing`.
