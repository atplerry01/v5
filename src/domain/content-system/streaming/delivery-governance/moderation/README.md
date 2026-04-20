# moderation (SCAFFOLD — pending implementation)

## Purpose

The `moderation` leaf owns **streaming-level moderation decisions** — flagging streams or broadcasts, reviewing the flag, and issuing allow/block/restrict verdicts that affect delivery.

## Owns (planned)

- Moderation decision identity, target ref (stream / broadcast / channel), flag reason, moderator identity, decision (Allow / Block / Restrict), rationale, status (Flagged / InReview / Decided / Overturned).
- Flag / assign / decide / overturn transitions.

## Does not own

- Content-policy rule definitions — governance/config concern.
- Enforcement — policy engine / runtime.
- Document-level or media-level moderation (those are owned by their respective context governance).

## Status

SCAFFOLD only in P2.6.CS.7.
