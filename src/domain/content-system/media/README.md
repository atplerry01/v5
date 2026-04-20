# Media

## Purpose

The `media` context owns the truth of rich-media content — the generic asset (absorbing former audio/video/image intrinsic descriptors), companions (subtitle, transcript), descriptor metadata, and the media-centric lifecycle (intake/ingest, technical-processing, version, rights/publication, safety/governance).

A "media asset" here is a content object, not a catalogue listing, not a programme, not a licensed good.

## Domain-groups (post CS.8 / CS.9)

- `core-object/` — the generic media asset + companions (subtitle, transcript) + scaffolded rendition, artwork, preview.
- `descriptor/` — descriptive metadata.
- `intake/` — external → system ingress (`ingest`; formerly `lifecycle/upload`).
- `technical-processing/` — processing, quality, integrity (analysis pipelines and measured/attested facts).
- `lifecycle-change/` — internal state transitions (version).
- `composition-catalog/` — (SCAFFOLD pending CS.10) package, sequence.
- `rights-publication/` — (SCAFFOLD pending CS.10) rights, publication.
- `safety-governance/` — (SCAFFOLD pending CS.10) moderation, accessibility.

**Removed groups:** `content-artifact/` (contents moved to core-object; audio/video/image/media-file retired), `companion-artifact/` (contents moved to core-object), `lifecycle/` (decomposed).

## Ownership boundaries

### Owns

- Media asset identity, title, classification, status (Draft / Active / Retired) with invariant that Active requires non-`Unclassified`.
- Intrinsic type-specific descriptors per §CD-03 (AudioDuration, VideoDimensions, FrameRate, ImageOrientation, etc.) on `core-object/asset/value-object/`.
- Companion artifacts: Subtitle (format, language, timing window), Transcript (format, language).
- Descriptor metadata.
- Ingest transaction truth (formerly Upload).
- Processing job truth.
- Version lineage.
- Evaluative/measured facts (bitrate, codec, quality) — HOME: `technical-processing/quality/` (scaffold).
- Integrity attestations — HOME: `technical-processing/integrity/` (SCAFFOLD pending CS.10).

### Does not own

- Catalogue / programme / commercial-product semantics — upstream.
- Rights/licensing DECISIONS — commercial/legal/policy layers (but rights INTENT lives in `rights-publication/` per target canonical).
- Player implementation, CDN behaviour, transcoder adapters — infrastructure.
- Retention — per §DF-04, per-context retention only; media does not currently model retention.
- Storage backend of media files — per §CD-04, `media-file` aggregate retired; `MediaFileRef` VOs remain as opaque infrastructure refs.

## Leaf domains (post CS.9)

- `core-object/asset` — generic media asset (plus absorbed intrinsic type VOs).
- `core-object/subtitle` — subtitle companion.
- `core-object/transcript` — transcript companion.
- `descriptor/metadata` — descriptive metadata.
- `intake/ingest` — media ingest transaction.
- `technical-processing/processing` — media processing job.
- `technical-processing/quality` — (SCAFFOLD) evaluative/measured facts.
- `lifecycle-change/version` — media version lineage.

### Pending CS.10 scaffolds (10)

- core-object/{rendition, artwork, preview}
- technical-processing/integrity
- composition-catalog/{package, sequence}
- rights-publication/{rights, publication}
- safety-governance/{moderation, accessibility}
