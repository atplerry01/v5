# Content Policy Domain

**Path:** `content-system/governance/content-policy`
**Namespace:** `Whycespace.Domain.ContentSystem.Governance.ContentPolicy`

## Purpose
Owns the governance document that defines content policies — drafted,
published, amended, and retired over time with a monotonically
increasing revision number.

## Lifecycle
```
Draft → Published → (repeatable Amend) → Retired (terminal)
```

## Events
- `ContentPolicyDraftedEvent`
- `ContentPolicyPublishedEvent`
- `ContentPolicyAmendedEvent`
- `ContentPolicyRetiredEvent`

## Invariants
1. Name required.
2. Revision number strictly increasing on amendment.
3. Retired is terminal.
