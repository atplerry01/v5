# document / governance

## Purpose

Groups **governance, compliance, and moderation** aggregates for the document context. Governance domains own retention, classification decisions, and moderation lifecycles.

## Why this group exists

Retention, classification, and moderation are compliance/policy concerns distinct from object truth (`core-object/`), ingress (`intake/`), internal state transitions (`lifecycle-change/`), and descriptive metadata (`descriptor/`). They share the semantic class of "policy-driven lifecycle aggregates that gate, restrict, or annotate a document on compliance grounds."

Per §DF-04 closure: retention remains PER-CONTEXT (not promoted to `shared/`). Media and streaming get their own retention domains if needed later.

## Leaf domains

- `retention/` — retention attachment and lifecycle (Applied / Held / Released / Expired / EligibleForDestruction / Archived). Target may be a document, record, or file.
- `classification/` — (SCAFFOLD pending CS.3) classification decision lifecycle — propose / apply / revise / revoke / challenge. See §CD-16 for disambiguation from the `DocumentClassification` VO on `core-object/document`.
- `moderation/` — (SCAFFOLD pending CS.3) document moderation decisions.

## Boundary notes

- Retention records eligibility for destruction — the actual destruction is orchestrated downstream.
- Classification DECISIONS live here; the currently-assigned classification value is cached as a VO on the document aggregate (§CD-16).
- Moderation decisions live here; the moderation action (block/allow/flag) is the aggregate's truth, not the enforcement mechanism.
