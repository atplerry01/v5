# Moderation Domain

**Path:** `content-system/governance/moderation`
**Namespace:** `Whycespace.Domain.ContentSystem.Governance.Moderation`

## Purpose
Tracks a moderation case against a piece of content: evidence, decision,
appeal, and closure.

## Lifecycle
```
Opened → UnderReview (evidence attached) → Decided → Appealed → Closed (terminal)
                                                    ↘ Closed
```

## Events
- `ModerationCaseOpenedEvent`
- `ModerationEvidenceAttachedEvent`
- `ModerationDecisionIssuedEvent`
- `ModerationCaseAppealedEvent`
- `ModerationCaseClosedEvent`

## Invariants
1. Target reference required.
2. Reporter reference required when opening.
3. Decision must be definitive (not Undecided).
4. Appeal requires prior decision.
5. Closed is terminal.
