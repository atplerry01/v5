# transcript

## Purpose

The `transcript` leaf owns transcript companion artifacts for a media asset — format, language, and the produced transcript output reference. Draft → Active → Finalized → Archived.

## Aggregate root

- `TranscriptAggregate`

## Key value objects

- `TranscriptId`
- `MediaAssetRef`
- `MediaFileRef` (optional — source file)
- `TranscriptFormat`
- `TranscriptLanguage`
- `TranscriptOutputRef`
- `TranscriptStatus` (Draft / Active / Finalized / Archived)

## Key events

- `TranscriptCreatedEvent`
- `TranscriptUpdatedEvent`
- `TranscriptFinalizedEvent`
- `TranscriptArchivedEvent`

## Invariants and lifecycle rules

- Created in `Draft` with optional source file.
- `Update` sets the `TranscriptOutputRef` and moves status to `Active`; rejected on finalized or archived.
- `Finalize` rejects already-finalized or archived.
- Archived is terminal.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedTranscript`).

## Owns

- Transcript identity, language, format, output ref, status.
- Create / update / finalize / archive transitions.

## References

- `MediaAssetRef` — the asset this transcript accompanies.
- `MediaFileRef` (optional) — the source media file this transcript was derived from.
- `TranscriptOutputRef` — the produced transcript artifact.

## Does not own

- The media asset — owned by `content-artifact/asset`.
- The ASR engine or transcription pipeline — infrastructure.
- Search indexing over transcript text — downstream.

## Notes

- Unlike `subtitle`, transcript does not carry a timing window at the leaf level — it is treated as a full-length companion document. Timing information, if needed, belongs in the `TranscriptOutputRef` target format.
