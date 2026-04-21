# upload

## Purpose

The `upload` leaf owns the transaction truth of a document upload — from request through acceptance, processing-start, and terminal completion, failure, or cancellation.

## Aggregate root

- `DocumentUploadAggregate`

## Key value objects

- `DocumentUploadId`
- `DocumentUploadSourceRef`
- `DocumentUploadInputRef`
- `DocumentUploadOutputRef`
- `DocumentUploadStatus` (Requested / Accepted / Processing / Completed / Failed / Cancelled)
- `DocumentUploadFailureReason`

## Key events

- `DocumentUploadRequestedEvent`
- `DocumentUploadAcceptedEvent`
- `DocumentUploadProcessingStartedEvent`
- `DocumentUploadCompletedEvent`
- `DocumentUploadFailedEvent`
- `DocumentUploadCancelledEvent`

## Invariants and lifecycle rules

- Linear ordered happy path: `Requested → Accepted → Processing → Completed`.
- `Accept` is valid only from `Requested`; `StartProcessing` only from `Accepted`; `Complete` only from `Processing`.
- `Fail` is valid from any non-terminal state; `Cancel` is valid from any non-terminal state.
- Terminal states (`Completed`, `Failed`, `Cancelled`) reject all further transitions (`AlreadyTerminal`, `CannotCancelTerminal`).

## Owns

- Upload transaction identity, input/output refs, status, failure reason, timeline (requested/accepted/started/completed).
- Request / accept / start-processing / complete / fail / cancel transitions.

## References

- `DocumentUploadSourceRef` — opaque source of the upload.
- `DocumentUploadInputRef` — opaque input bytes reference.
- `DocumentUploadOutputRef` — produced output reference (attached on completion).

## Does not own

- The document or file produced by the upload — owned by `document/core-object/document` and `document/core-object/file`.
- The actual transfer mechanism (multipart, resumable, signed-URL) — infrastructure.
- Retry policy for failures — that belongs to the orchestration/runtime layer.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanCompleteDocumentUploadSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `DocumentUploadAggregate.Request(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
