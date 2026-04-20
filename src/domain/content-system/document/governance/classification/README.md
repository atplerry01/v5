# classification (SCAFFOLD — pending implementation)

## Purpose

The `classification` leaf owns the **classification decision lifecycle** for a document — proposing a classification, applying it, revising it, revoking it, or challenging a prior decision. This aggregate is the source of truth for WHO classified WHAT, WHEN, and WHY.

## Owns

- Classification decision identity, target ref (DocumentRef), proposed classification, applied classification, reviewer/approver identity, rationale, status (Proposed / Applied / Revised / Revoked / Challenged).
- Propose / apply / revise / revoke / challenge transitions.
- The decision audit trail.

## Does not own

- The currently-ASSIGNED classification value on the document — that is cached on `DocumentAggregate.DocumentClassification` VO (§CD-16).
- Classification taxonomy definition — that is a governance/config concern.
- Enforcement of classification (who can see what) — policy/identity layer.

## §CD-16 disambiguation (mandatory)

Two concerns, two owners, zero collision:

- **`document/governance/classification`** (THIS DOMAIN, aggregate) — owns the decision lifecycle.
- **`document/core-object/document.DocumentClassification`** (VO) — owns the currently-assigned value as cached state on the document aggregate. Mutated ONLY in reaction to events from this domain (`ClassificationAppliedEvent`, `ClassificationRevisedEvent`, `ClassificationRevokedEvent`).

DocumentAggregate MUST NOT mutate its `DocumentClassification` VO via local commands — that is a boundary bleed.

## Status

SCAFFOLD only in P2.6.CS.3.
