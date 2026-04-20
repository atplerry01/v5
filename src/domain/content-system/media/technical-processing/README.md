# media / technical-processing

## Purpose

Groups **technical processing, quality analysis, and integrity attestation** aggregates for the media context.

## Leaf domains

- `processing/` — media processing job (transcoding, probing, render). Moved from `lifecycle/processing` in CS.9.
- `quality/` — (SCAFFOLD from CS.8) evaluative / measured technical assessments per §CD-03 refined rule.
- `integrity/` — (SCAFFOLD pending CS.10) integrity attestation for media files.

## Boundary notes

- Intrinsic facts (duration, dimensions, format) live on `core-object/asset`, not here — see §CD-03 decision rule in `asset/README.md`.
- Quality and integrity share the "derived/attested facts" nature — both record results of analysis against the media bytes.
