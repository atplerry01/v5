---
classification: content-system
context: cross-domain
domain: extraction-and-enforcement
stored_at: 2026-04-21T01:03:48Z
---

# TITLE
Content-System Extraction + Enforcement Pass — separate, normalize, lock

# CONTEXT
Content currently lives embedded in business / other-classification aggregates
as raw string fields (description, notes, body, comments), document/blob
structures, and metadata objects. WBSM v3 requires content to be externalised
into its own classification system, owned structurally, versioned, immutable,
and lifecycle-managed. Business aggregates reference content only via
`ContentId`.

`src/domain/content-system/` already exists per the project-memory
content-system rollout (Phase 1–3: media/asset → messaging → course → …).
This pass respects that rollout and must not collide with it.

# OBJECTIVE
Complete the separation and establish enforcement:
- Externalise content from business aggregates.
- Version + immutability + lifecycle on `ContentAggregate`.
- Structural-owner binding (not business-owner).
- Arch-test lock against regression.

# CONSTRAINTS
- No external document system.
- No mutable content.
- Content never binds directly to business aggregates — only via `ContentId`.
- No event-schema breaks.
- No duplicate content representations.

# EXECUTION STEPS
1. **Phase 1 — Content Inventory** (MANDATORY, reports then STOPS).
2. Phase 2 — `ContentAggregate` + `ContentVersion`.
3. Phase 3 — lifecycle model (Draft / Active / Archived / Superseded).
4. Phase 4 — versioning (immutable).
5. Phase 5 — structural binding.
6. Phase 6 — business-aggregate decoupling (replace embedded content with `ContentId`).
7. Phase 7 — event model.
8. Phase 8 — migration per aggregate.
9. Phase 9 — enforcement lock (arch-tests).

# OUTPUT FORMAT
- Phase 1 inventory: table of {Aggregate, Field, ContentType, NeedsVersioning}.
- Subsequent phases only after explicit user approval.

# VALIDATION CRITERIA
- Inventory covers every embedded-content site in `src/domain/**`.
- Classification is unambiguous per row.
- Pass halts after inventory per user instruction ("Stop after inventory and
  report before proceeding").
