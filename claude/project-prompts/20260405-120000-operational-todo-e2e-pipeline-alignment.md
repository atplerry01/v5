# TITLE
WBSM v3.5 — TODO E2E Pipeline Alignment (Systems-First, Full Flow Compliance)

# CLASSIFICATION
operational / sandbox / todo

# CONTEXT
Upgrade Todo vertical slice to fully comply with canonical WBSM v3.5 execution flow.
The Todo domain is the first canonical reference implementation for all future domains.

# OBJECTIVE
Enforce the canonical flow: Platform API → Systems.Downstream → Systems.Midstream (optional) → Runtime Control Plane → T0U (Policy/Guard) → T1M (Workflow) → T2E (Engine) → Domain Aggregate → Domain Events → Runtime (Persist → Chain → Kafka → Projection) → Systems → Platform API Response.

# CONSTRAINTS
- Platform API MUST NOT call Runtime or Engines directly
- Systems MUST own intent routing
- Engines MUST NOT persist
- Domain MUST remain pure
- Runtime is ONLY layer allowed to persist/anchor/publish
- Policy MUST execute BEFORE any domain load
- Projection MUST be triggered AFTER persistence

# EXECUTION STEPS
11 batches executed: Systems.Downstream creation, Midstream verification, Platform API update, Runtime pipeline reorder, Engine/Domain/Projection verification, Policy definition, E2E test creation.

# OUTPUT FORMAT
Code changes across Systems, Platform, Runtime, Projections, and Tests layers.

# VALIDATION CRITERIA
- All 18 tests pass (11 unit + 7 integration including 3 E2E)
- No direct API → Runtime calls
- Systems layer active
- Policy executes before domain
- Engine stateless
- Domain pure
- Runtime owns persistence + chain + Kafka
- Projection updated
- End-to-end flow traceable
