# processing

## Purpose

The `processing` leaf owns the transaction truth of a document processing job — OCR, extraction, redaction, conversion, render, or any other typed processing kind. It carries the job through requested → running → terminal outcome.

## Aggregate root

- `DocumentProcessingAggregate`

## Key value objects

- `ProcessingJobId`
- `ProcessingKind`
- `ProcessingInputRef`
- `ProcessingOutputRef`
- `ProcessingStatus` (Requested / Running / Completed / Failed / Cancelled)
- `ProcessingFailureReason`

## Key events

- `DocumentProcessingRequestedEvent`
- `DocumentProcessingStartedEvent`
- `DocumentProcessingCompletedEvent`
- `DocumentProcessingFailedEvent`
- `DocumentProcessingCancelledEvent`

## Invariants and lifecycle rules

- Linear ordered happy path: `Requested → Running → Completed`.
- `Start` is valid only from `Requested`; `Complete` is valid only from `Running`.
- `Fail` and `Cancel` are valid from any non-terminal state.
- Terminal states reject all further transitions.

## Owns

- Processing job identity, kind, input/output refs, status, failure reason, timeline.
- Request / start / complete / fail / cancel transitions.

## References

- `ProcessingInputRef` — opaque input reference (e.g. a file).
- `ProcessingOutputRef` — opaque output reference (e.g. an extracted text blob, a rendered PDF).

## Does not own

- The actual OCR / extraction / rendering engine — infrastructure.
- Resource allocation, queue prioritisation — orchestration/runtime.
- The document or file that was processed — owned by their respective leaves.

## Notes

- `ProcessingKind` encodes the variety of processing (OCR, extraction, render, conversion, redaction). The aggregate treats all kinds uniformly — only the kind value differs.
