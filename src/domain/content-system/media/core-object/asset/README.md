# asset

## Purpose

The `asset` leaf owns the generic media asset — its identity, title, classification, draft/active/retired lifecycle, AND (after P2.6.CS.8) the **intrinsic type-specific descriptors** formerly held by the retired Audio / Video / Image aggregates. It is the canonical home for all type-specific technical facts inherent to a media asset.

## Aggregate root

- `AssetAggregate` — holds the `AssetKind` discriminator (added in CS.13A Wave 6). `Kind` defaults to `AssetKind.Other` on creation; an explicit `AssignKind(newKind, assignedAt)` command transitions it. Kind-qualified intrinsic VO WIRING (e.g., asserting "this asset has a `ChannelCount` only if Kind == Audio") is NOT enforced by the aggregate — callers are expected to honour the §CD-03 split rule when constructing values. Future invariant tightening would add kind-VO consistency checks.

## Key value objects

### Asset-level

- `AssetId`
- `AssetTitle`
- `AssetClassification`
- `AssetStatus` (Draft / Active / Retired)
- `AssetKind` (Other / Audio / Video / Image) — NEW in CS.13A Wave 6.

### Intrinsic type-specific (absorbed in CS.8 per §CD-03 refined rule)

- Audio: `AudioDuration`, `AudioFormat`, `ChannelCount`
- Video: `VideoDuration`, `FrameRate`, `VideoDimensions`
- Image: `ImageDimensions`, `ImageOrientation`

Note: `VideoFormat` did not exist as a separate VO in the pre-merge codebase; codec/container facts were encoded within the discriminator implicitly. If surfaced, a future `VideoFormat` VO lands here.

These VOs live under `asset/value-object/` and are available for use at the application layer alongside `AssetKind`. As of CS.13A, `AssetAggregate` holds `AssetKind` but does NOT store the kind-qualified VO values on its state — they are expected to flow through processing/quality aggregates and be read from there. Future evolution may absorb selected intrinsic VOs onto `AssetAggregate`'s state with kind-consistency invariants.

## §CD-03 refined split rule (mandatory reference)

Two homes for media descriptive facts:

1. **Intrinsic (inherent to the kind)** → STAYS HERE on `asset/`:
   - Structural facts that would not change if the bytes were re-analysed with a different tool.
   - Audio duration, channel count, audio format; video duration, frame rate, dimensions; image dimensions, orientation.

2. **Evaluative / derived (measured from bytes)** → `media/technical-processing/quality/`:
   - Facts that could change with a different analyzer or settings.
   - Bitrate, codec, sample-rate, PSNR, SSIM, loudness LUFS, peak levels, perceived-quality scores, probe/scan results, detected anomalies.

Decision rule for any new VO: "could this value change if we re-analyse the same bytes with a different tool / settings?" YES → quality. NO → asset.

## Key events

- `AssetCreatedEvent`
- `AssetRenamedEvent`
- `AssetReclassifiedEvent`
- `AssetActivatedEvent`
- `AssetRetiredEvent`
- `AssetKindAssignedEvent` — NEW in CS.13A Wave 6.

Audio/Video/Image `*Created` / `*Updated` / `*Activated` / `*Archived` events have been RETIRED along with their aggregates in CS.8. `AssetCreatedEvent` is intentionally UNCHANGED — the event shape does not include `AssetKind`, to preserve decodability of any prior-persisted `AssetCreatedEvent` streams. Kind is assigned via a separate `AssignKind` command emitting `AssetKindAssignedEvent`.

## Invariants and lifecycle rules

- A newly created asset starts in `Draft`.
- A retired asset is immutable.
- `Rename` and `Reclassify` are no-ops when the new value matches current.
- `EnsureInvariants` requires that an active asset is NOT `AssetClassification.Unclassified`.

## Owns

- Asset identity, title, classification, status.
- AssetKind discriminator (Other/Audio/Video/Image) plus the file-set of intrinsic kind-qualified VOs.
- `AssignKind` transition — assigning a new kind requires the asset to be non-retired and the new kind to differ from current.

## Does not own

- Evaluative / measured technical facts — owned by `media/technical-processing/quality/`.
- Backing file bytes — owned by `media-file` (retires in CS.9 per §CD-04; VO ref preserved).
- Metadata entries — owned by `descriptor/metadata`.
- Upload, processing, version lifecycle — owned by `intake/`, `technical-processing/`, and `lifecycle-change/` groups.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanRetireSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `AssetAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
