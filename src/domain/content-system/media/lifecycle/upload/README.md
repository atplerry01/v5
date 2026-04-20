# upload

## Purpose

The `upload` leaf owns the transaction truth of a media upload — from request through acceptance, processing-start, and terminal completion, failure, or cancellation.

## Aggregate root

- `MediaUploadAggregate`

## Key value objects

- `MediaUploadId`
- `MediaUploadSourceRef`
- `MediaUploadInputRef`
- `MediaUploadOutputRef`
- `MediaUploadStatus` (Requested / Accepted / Processing / Completed / Failed / Cancelled)
- `MediaUploadFailureReason`

## Key events

- `MediaUploadRequestedEvent`
- `MediaUploadAcceptedEvent`
- `MediaUploadProcessingStartedEvent`
- `MediaUploadCompletedEvent`
- `MediaUploadFailedEvent`
- `MediaUploadCancelledEvent`

## Invariants and lifecycle rules

- Linear ordered happy path: `Requested → Accepted → Processing → Completed`.
- `Accept` is valid only from `Requested`; `StartProcessing` only from `Accepted`; `Complete` only from `Processing`.
- `Fail` and `Cancel` are valid from any non-terminal state.
- Terminal states reject all further transitions.

## Owns

- Upload transaction identity, input/output refs, status, failure reason, timeline.
- Request / accept / start-processing / complete / fail / cancel transitions.

## References

- `MediaUploadSourceRef` — opaque source of the upload.
- `MediaUploadInputRef` — opaque input bytes reference.
- `MediaUploadOutputRef` — produced output reference (typically a `MediaFileRef`-shaped handle, attached on completion).

## Does not own

- The media asset or media file produced — owned by `content-artifact/asset` and `content-artifact/media-file`.
- The actual transfer mechanism (chunked, resumable, signed-URL) — infrastructure.
- Retry policy for failures — runtime concern.

## Notes

- This aggregate mirrors `document/lifecycle/upload` in shape and status transitions; the two are deliberately parallel to keep upload semantics consistent across content contexts.
