# accessibility (SCAFFOLD — pending implementation)

## Purpose

The `accessibility` leaf owns **media accessibility compliance tracking** — captions present, audio-description available, transcript available, WCAG-relevant attestations for a media asset.

## Owns (planned)

- Accessibility record identity, asset ref, captions-ref, audio-description-ref, transcript-ref, compliance-status (Compliant / PartiallyCompliant / NonCompliant / Unknown).
- Attest / revise / supersede transitions.

## Does not own

- The captions / audio description / transcript artifacts themselves — owned by `core-object/subtitle`, companion audio-description aggregate (if introduced), `core-object/transcript`.

## Status

SCAFFOLD only in P2.6.CS.10.
