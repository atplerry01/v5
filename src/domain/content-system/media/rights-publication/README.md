# media / rights-publication

## Purpose

Groups **media rights intent and publication lifecycle** aggregates.

## Leaf domains

- `rights/` — (SCAFFOLD) media rights intent aggregate. Owns rights metadata attached to an asset — territory, window, rightsholder attribution, usage permissions. NOT the authoritative legal grant — that lives upstream; this domain records the CLAIMED rights state as relevant to media distribution.
- `publication/` — (SCAFFOLD) media publication lifecycle. Distinct from `lifecycle-change/version` — publication is a visibility/availability commitment.

## Status

SCAFFOLD only in P2.6.CS.10.
