# quality (SCAFFOLD — technical-processing/quality)

## Purpose

The `quality` leaf is the canonical home for **evaluative / derived technical assessments** of a media asset per §CD-03 refined rule. Unlike intrinsic descriptors (which sit on `media/core-object/asset`), quality measurements are facts PRODUCED by analysis pipelines and could differ if re-analysed with a different tool or settings.

## §CD-03 refined split rule

- Intrinsic VOs (duration, channel count, frame rate, dimensions) → `media/core-object/asset/value-object/`.
- Evaluative VOs (bitrate, codec, sample-rate, quality scores, loudness, probe results) → HERE (`technical-processing/quality/value-object/`).

## Current status — scaffold-only

The pre-CS.8 media codebase had NO pure evaluative VOs. All existing type-specific VOs classified as intrinsic and were absorbed into `asset/`. Quality is therefore **scaffolded** in CS.8 but the 7 artifact subfolders sit empty (`.gitkeep` placeholders only). When a feature phase introduces real quality measurements, they populate here as:

- `value-object/QualityMeasurement.cs` — typed measurement (key + value + unit + analyzer-ref).
- `aggregate/QualityAggregate.cs` — (optional) aggregate holding a measurement batch for an asset.
- `event/QualityMeasuredEvent.cs` — measurement-recorded event.

## Boundary notes

- Quality measurements reference an asset via an opaque `AssetRef`, same pattern as other media leaves.
- Re-measurement replaces or supersedes prior measurements; measurement history is kept per the quality aggregate's lifecycle (when implemented).

## Status

SCAFFOLD only in P2.6.CS.8. Implementation deferred to a feature phase.
