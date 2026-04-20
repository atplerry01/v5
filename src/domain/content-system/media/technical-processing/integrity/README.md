# integrity (SCAFFOLD — pending implementation)

## Purpose

The `integrity` leaf owns **media integrity attestation lifecycle** — computing a checksum on the media bytes, comparing to declared checksum, and issuing attestations. Mirrors `document/integrity-provenance/integrity` for the media context.

## Owns (planned)

- Attestation identity, asset ref, declared checksum, computed checksum, verifier identity, verification timestamp, status (Pending / Verified / Mismatched / Revoked).

## Does not own

- Asset bytes — infrastructure.
- The checksum algorithm — infrastructure.

## Status

SCAFFOLD only in P2.6.CS.10.
