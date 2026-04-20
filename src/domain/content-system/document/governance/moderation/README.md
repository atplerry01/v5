# moderation (SCAFFOLD — pending implementation)

## Purpose

The `moderation` leaf owns **document moderation decisions** — flagging, reviewing, and issuing allow/block/restrict verdicts on documents that trip content-policy rules.

## Owns

- Moderation decision identity, target ref (DocumentRef), flag reason, moderator identity, decision (Allow / Block / Restrict), rationale, status (Flagged / InReview / Decided / Overturned).
- Flag / assign / decide / overturn transitions.

## Does not own

- Content-policy rule definitions — governance/config concern.
- Enforcement mechanism — policy/identity/runtime layer.
- Author/owner attribution of the flagged document — owned by `core-object/document` and upstream identity.

## Status

SCAFFOLD only in P2.6.CS.3.
