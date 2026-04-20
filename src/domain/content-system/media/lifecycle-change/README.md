# media / lifecycle-change

## Purpose

Groups **internal state transitions** applied to a media asset after it is owned by the system.

## Leaf domains

- `version/` — media asset version lineage (Draft → Active → Superseded / Withdrawn). Moved from `lifecycle/version` in CS.9.

## Boundary notes

- Distinct from `intake/` (ingress) and `technical-processing/` (analysis pipelines).
- Version is first-class per §CD-02; future lifecycle-change domains (review, publication if needed) would land here.
