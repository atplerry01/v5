# file

## Purpose

The `file` leaf owns the stored byte truth for a document: where the bytes live, the declared checksum, the MIME type, the size, the integrity-verification state, and the registered/superseded/archived status.

## Aggregate root

- `DocumentFileAggregate`

## Key value objects

- `DocumentFileId`
- `DocumentRef`
- `DocumentFileStorageRef`
- `DocumentFileChecksum`
- `DocumentFileMimeType`
- `DocumentFileSize`
- `DocumentFileIntegrityStatus` (Unverified / Verified)
- `DocumentFileStatus` (Registered / Superseded / Archived)

## Key events

- `DocumentFileRegisteredEvent`
- `DocumentFileIntegrityVerifiedEvent`
- `DocumentFileSupersededEvent`
- `DocumentFileArchivedEvent`

## Invariants and lifecycle rules

- A file is registered with a declared checksum; integrity is `Unverified` until a computed checksum is submitted.
- `VerifyIntegrity` requires the computed checksum to equal the declared checksum, or `ChecksumMismatch` is raised.
- Double-verification is rejected.
- An archived or superseded file cannot be verified again.
- A file cannot supersede itself.
- `DocumentRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedDocumentFile`).

## Owns

- Byte-location truth (`DocumentFileStorageRef`), declared checksum, MIME, size.
- Integrity verification state and supersession chain (`SuccessorFileId`).
- Register / verify / supersede / archive transitions.

## References

- `DocumentRef` — back-pointer to the owning document.
- `DocumentFileStorageRef` — opaque storage reference; the file aggregate does not know about storage backends.

## Does not own

- The document itself (identity, title, classification) — owned by `document/core-object/document`.
- The storage backend, the byte-fetch mechanism, or the checksum algorithm — those are infrastructure.
- Virus scanning, redaction, or OCR — those are driven out of `document/lifecycle/processing`.

## Notes

- Supersession links a file to its successor via `SuccessorFileId`. The aggregate does not construct the supersession chain — callers supply the successor id.
