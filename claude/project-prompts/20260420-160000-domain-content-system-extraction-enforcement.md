---
classification: domain
context: content-system
domain: content-extraction-enforcement
status: inventory-only
---

# Content-System Extraction and Enforcement Pass (Phase 1 Inventory)

## TITLE
Content-System Extraction — Inventory of Embedded Content Across Domain

## CONTEXT
`src/domain/content-system/` already exists with media, document, and streaming contexts. This pass is NOT a new subsystem. It is a **separation of content artifacts from business aggregates** and an enforcement of lifecycle + versioning discipline on content.

User instruction: **Start with Phase 1 inventory. Do NOT implement until inventory is complete and reviewed.**

## OBJECTIVE (Phase 1 only)
Enumerate every content-like field across `src/domain/**` outside `content-system/**`, classify as embedded / external / duplicated, and produce a Content Inventory that calls out:
- source aggregate
- content type
- current storage pattern
- versioning requirement

## CONSTRAINTS
- Inventory only. No code changes this turn.
- Exclude `content-system/**` from "drift" — that's the target location.
- Exclude `shared-kernel/**` primitives (Label, Description) unless embedded inline in an aggregate body.
- Distinguish between (a) genuine business-aggregate content leakage and (b) domain-local identifier / status / reason strings that are NOT content (those are structural).

## EXECUTION STEPS
1. Enumerate existing `content-system/**` aggregates + VOs to know the target.
2. Scan `src/domain/{business,operational,economic,constitutional,trust,decision,integration,orchestration,structural,core}-system/**/*Aggregate.cs` for content-like string / binary fields.
3. Scan entities + value objects in the same tree for same.
4. Cross-check against `content-system/**` for duplicates.
5. Classify + tabulate.

## OUTPUT FORMAT
- Per-occurrence row: classification, context, aggregate, field, pattern, versioning-required (Y/N), disposition (extract / already-external / leave-as-structural).
- Summary by classification.
- Duplication findings.

## VALIDATION CRITERIA
- Every aggregate in scope touched by scan.
- No premature implementation.
- Disposition column justified (not just "extract" blindly).
