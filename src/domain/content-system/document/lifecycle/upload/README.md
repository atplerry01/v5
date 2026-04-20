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

- The document or file produced by the upload — owned by `document/content-artifact/document` and `document/content-artifact/file`.
- The actual transfer mechanism (multipart, resumable, signed-URL) — infrastructure.
- Retry policy for failures — that belongs to the orchestration/runtime layer.
