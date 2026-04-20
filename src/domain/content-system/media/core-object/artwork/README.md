# artwork (SCAFFOLD — pending implementation)

## Purpose

The `artwork` leaf owns **associated artwork** for an asset — thumbnail, cover, poster, still. Distinct from the asset's content bytes; artwork is a decorative/identifying image attached to an asset.

## Owns (planned)

- Artwork identity, asset ref, artwork kind (Thumbnail / Cover / Poster / Still), output ref, status.
- Register / replace / retire transitions.

## Does not own

- The artwork image bytes themselves — infrastructure storage.

## Status

SCAFFOLD only in P2.6.CS.10.
