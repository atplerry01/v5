# TITLE

Phase 1.6 — Portfolio Batch 2 S4 Domain Implementation (performance, rebalance, benchmark, mandate)

# CONTEXT

Classification: business-system
Context: portfolio
Domains: performance, rebalance, benchmark, mandate
Prior maturity: S1 (stubs)
Target maturity: S4 (complete)

# OBJECTIVE

Upgrade performance, rebalance, benchmark, and mandate domains from S1 → S4 with full event-sourced aggregates, typed value objects, domain-specific events, specifications, errors, and README documentation.

# CONSTRAINTS

* No financial calculations
* No performance computation engines
* No rebalancing execution
* No optimization algorithms
* No external data feeds
* No Guid.NewGuid()
* No DateTime.UtcNow()
* Zero external dependencies
* Domain defines behavior contracts only

# EXECUTION STEPS

1. Load all guards from /claude/guards/
2. Read existing S1 stubs and S4 reference (portfolio domain)
3. Implement typed value objects (Id with validation, Status enum, Name)
4. Implement domain-specific events with typed data
5. Implement domain errors with factory methods
6. Implement specifications (lifecycle transition guards)
7. Implement event-sourced aggregates with Apply/AddEvent/EnsureInvariants
8. Update READMEs with invariants, lifecycle, boundary statement
9. Run audit sweep
10. Validate output

# OUTPUT FORMAT

Summary, files list, tracker, audit results

# VALIDATION CRITERIA

* Performance: REVERSIBLE lifecycle (Draft → Active ↔ Suspended → Closed)
* Rebalance: REVERSIBLE lifecycle (Draft → Pending → Approved/Rejected, Rejected → Pending)
* Benchmark: TERMINAL lifecycle (Draft → Active → Retired)
* Mandate: TERMINAL lifecycle (Draft → Enforced → Revoked)
* All aggregates event-sourced with Version tracking
* All IDs validated (no empty Guid)
* All specifications pure
* All services stateless
* No external dependencies
* Deterministic
