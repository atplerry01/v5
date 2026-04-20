# Media

## Purpose

The `media` context owns the truth of rich-media content artifacts — the generic asset, its typed specialisations (audio, video, image), the underlying media-file bytes, companion artifacts (subtitle, transcript), descriptor metadata, and the media-centric lifecycle (upload, processing, version).

A "media asset" here is a content object, not a catalogue listing, not a programme, not a licensed good.

## Domain-groups

- `content-artifact/` — the generic media asset plus its typed specialisations (audio, video, image) and the underlying media-file bytes.
- `companion-artifact/` — artifacts that exist to accompany a media asset rather than being the asset itself: subtitle, transcript.
- `descriptor/` — descriptive metadata attached to a media asset. Typed key/value entries with finalize semantics.
- `lifecycle/` — cross-cutting lifecycle aggregates that move media assets through ingestion, transformation, and version progression.

## Ownership boundaries

### Owns

- Media asset identity, title, classification, status (Draft / Active / Retired) and the invariant that an Active asset must carry a non-`Unclassified` classification.
- Typed media specialisations: Audio (format, duration, channel count), Video (dimensions, duration, frame rate), Image (dimensions, orientation).
- Media-file byte truth: storage reference, declared and computed checksum, MIME, size, integrity / registration / corrupt / superseded states.
- Companion artifacts: Subtitle (format, language, timing window, output ref, finalize), Transcript (format, language, output ref, finalize).
- Descriptor metadata: typed key/value entries on a media asset, with add / update / remove / finalize.
- Media upload transaction truth.
- Media processing job truth (transcoding, packaging, thumbnail generation, normalisation — all modelled as a generic processing kind).
- Media version lineage.

### Does not own

- Catalogue, programming schedule, or commercial-product semantics.
- Rights/licensing decisions — belongs to policy, commercial, or legal layers.
- Player implementation, CDN behaviour, or transcoder adapters — those are infrastructure.
- Retention — not currently modelled in `media` (document retention is modelled separately under `document/lifecycle/retention`).

## Leaf domains

- `content-artifact/asset` — generic media asset aggregate.
- `content-artifact/audio` — typed audio specialisation.
- `content-artifact/video` — typed video specialisation.
- `content-artifact/image` — typed image specialisation.
- `content-artifact/media-file` — stored media-file bytes truth.
- `companion-artifact/subtitle` — subtitle companion artifact.
- `companion-artifact/transcript` — transcript companion artifact.
- `descriptor/metadata` — descriptive metadata entries attached to a media asset.
- `lifecycle/upload` — media upload transaction.
- `lifecycle/processing` — media processing job.
- `lifecycle/version` — media version lineage.
