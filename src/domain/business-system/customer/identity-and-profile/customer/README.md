# Customer

**Classification:** business-system
**Context:** customer
**Domain-Group:** identity-and-profile
**Domain:** customer
**Namespace:** `Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer`

## Responsibility
Business customer — the party to whom offerings are sold. Distinct from identity/auth account.

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities (none required at E1)
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services (none required at E1)
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented. Aggregate + VOs + events + specs + errors.
(superseded below)

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, customer, customer)`; the domain-group is a folder-level grouping and is not part of the route.
