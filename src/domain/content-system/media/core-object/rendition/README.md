# rendition (SCAFFOLD — pending implementation)

## Purpose

The `rendition` leaf owns **derived encoded variants** of an asset — a transcoded rendition at a particular bitrate/codec/container tuple. Distinct from the asset itself and from `technical-processing/processing/` (which is the JOB that produces renditions).

## Owns (planned)

- Rendition identity, asset ref, rendition profile (bitrate + codec + container + resolution), output ref, status (Requested / Rendering / Available / Failed / Retired).
- Request / render / publish / fail / retire transitions.

## Does not own

- The rendering engine — infrastructure.
- The asset itself — `core-object/asset`.
- Quality measurements of the rendition — `technical-processing/quality`.

## Status

SCAFFOLD only in P2.6.CS.10.
