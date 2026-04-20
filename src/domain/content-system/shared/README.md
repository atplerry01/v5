# content-system / shared

## Purpose

Holds **truly cross-context content truth** — concepts whose ownership spans `document`, `media`, and `streaming` and cannot legitimately live inside any one of them.

## NOT a dumping ground

The shared context is **gated**. Before adding a new group or leaf here, justify in writing:

1. Does this concept exist in EVERY content context (document, media, streaming)? If it only spans two, or is only relevant to one, it belongs inside that context.
2. Would putting it inside any one context force the other contexts to depend across boundaries? If yes, shared is justified. If no, shared is not justified.
3. Is this a CROSS-CONTEXT CONTENT TRUTH concern, not a cross-system concern? (Identity, access, commerce concerns belong in THEIR OWN classifications, not in content-system/shared.)

Additions to `shared/` require explicit review against the above three tests. Half-justified additions default to REJECTED.

## Canonical groups

- `relationship/` — cross-context content graph edges (document-cites-media, media-part-of-stream, stream-archives-broadcast).
- `localization/` — content localization variants shared across contexts.
- `provenance-evidence/` — evidence-linkage across content (not to be confused with document/integrity-provenance/provenance which is document-local custody).

## Leaf domains

- `relationship/relationship` — relationship aggregate.
- `localization/localization` — localization aggregate.
- `provenance-evidence/evidence` — evidence aggregate.

## Boundary notes

- Retention is **NOT** here (§DF-04 closed OPT-A — retention is per-context under `{context}/governance/retention`).
- Classification, moderation are **NOT** here — they are per-context governance.
- Rights, entitlement-hook are **NOT** here — they live in media and streaming respectively.

## Status

Scaffolded in P2.6.CS.5.0. All 3 leaves are placeholder-only; implementation deferred to the feature phase that needs them.
