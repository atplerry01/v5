# TITLE
Phase 2 — First Structured System Flow: Cluster to Ledger

# CONTEXT
Classification: structural-system, business-system, economic-system (cross-domain flow)
Context: cluster, subscription, billing, revenue, transaction, ledger
Domain: cluster, subcluster, spv, provider, enrollment, usage, bill-run, invoice, revenue, distribution, payout, transaction, journal, entry

Phase 2 objective: prove WhyceSpace works as a connected system by mapping the first end-to-end structured flow from structural topology through economic finalization.

# OBJECTIVE
1. Define domain interaction sequence from Cluster to Ledger
2. Define commands at each step
3. Define events emitted at each step
4. Define ownership boundaries (aggregate per step)
5. Define how structure drives execution
6. Define data references between domains
7. Define validation rules between structure and economics

# CONSTRAINTS
- Use existing S4 domains ONLY — no new domains
- No execution logic — interaction design only
- All cross-BC communication via domain events (domain.guard Rule 13)
- Revenue cannot write to ledger directly (economic.guard R4/D49)
- Transaction required between payout and ledger (economic.guard R5/D50)
- Transaction requires instruction (economic.guard T5/D43)
- Ledger journal must balance (economic.guard ECON-LEDGER-01)
- All state changes emit events (GE-04)
- All mutations pass WHYCEPOLICY (GE-02)
- All critical actions chain-anchored (GE-03)

# EXECUTION STEPS
1. Inventory all existing domains along the flow path
2. Map aggregate boundaries per step
3. Define command→event pairs per aggregate
4. Define cross-domain event subscriptions
5. Define data references (IDs carried across boundaries)
6. Define structural→economic validation rules
7. Produce canonical flow diagram

# OUTPUT FORMAT
Structured markdown with: flow steps, commands, events, aggregates, data references, validation rules

# VALIDATION CRITERIA
- Every step maps to an existing domain under src/domain/
- No cross-BC direct type references
- Revenue→Ledger path goes through Transaction context
- All events follow {Subject}{PastTenseVerb}Event naming
- All aggregates follow {Name}Aggregate naming
- Flow is deterministic and reproducible