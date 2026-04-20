# media-file

## Purpose

The `media-file` leaf owns the stored byte truth behind a media asset — where the bytes live, the declared checksum, the MIME type, the size, and the integrity/registration state including an explicit corrupt-marker.

## Aggregate root

- `MediaFileAggregate`

## Key value objects

- `MediaFileId`
- `StorageReference`
- `MediaChecksum`
- `MediaMimeType`
- `FileSize`
- `FileIntegrityStatus` (Unverified / Verified / Corrupt)
- `FileRegistrationStatus` (Registered / Superseded)

## Key events

- `MediaFileRegisteredEvent`
- `MediaFileIntegrityVerifiedEvent`
- `MediaFileMarkedCorruptEvent`
- `MediaFileSupersededEvent`

## Invariants and lifecycle rules

- Registered with a declared checksum; integrity starts `Unverified`.
- `VerifyIntegrity` requires the computed checksum to equal the declared checksum; mismatch raises `ChecksumMismatch`. Rejected if already verified, corrupt, or superseded.
- `MarkCorrupt` requires a non-empty reason; double-mark-corrupt rejected.
- `Supersede` rejects already-superseded or self-supersede.

## Owns

- Storage reference, declared checksum, MIME, size, integrity state, registration state, successor linkage.
- Register / verify / mark-corrupt / supersede transitions.

## References

- `StorageReference` — opaque storage reference; the aggregate does not know about storage backends.

## Does not own

- Any media asset — the file is asset-agnostic at its own level and does not carry `MediaAssetRef`. Typed specialisations (audio/video/image) and versions attach it to an asset via `MediaFileRef`.
- The transfer / upload mechanism — owned by `media/lifecycle/upload` and infrastructure.
- Transcoding / re-encoding — owned by `media/lifecycle/processing`.

## Notes

- Unlike `document/content-artifact/file` which holds a mandatory `DocumentRef`, the media file is deliberately not tied to a specific asset at this leaf — one file can serve many specialisations.
