# Account

**Classification:** business-system
**Context:** customer
**Domain-Group:** identity-and-profile
**Domain:** account
**Namespace:** `Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account`

## Responsibility
Business customer account (business meaning only — not identity/auth/ledger account).

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented (Batch 2). Aggregate + VOs + events + specs + errors.

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, customer, account)`; the domain-group is a folder-level grouping and is not part of the route.
