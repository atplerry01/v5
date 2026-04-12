# TITLE
Economic Enforcement Context Restructure — Rule + Violation Domains

# CONTEXT
Classification: economic-system
Context: enforcement
Domain: rule, violation
Phase: Domain scaffold (D1 activation)

# OBJECTIVE
Replace the generic single-domain enforcement scaffold with two purpose-built domains: `rule` (defines enforceable economic rules) and `violation` (records rule breaches). Establish event-sourced aggregates, value objects, events, errors, services, and specifications for both.

# CONSTRAINTS
- Enforcement does not mutate financial truth
- Every violation must reference a rule and a source
- Rules must be uniquely identifiable
- Violations must not exist without a triggering condition
- Domain layer has zero external dependencies (SharedKernel only)
- All state changes via domain events
- No Guid.NewGuid(), DateTime.UtcNow, or non-deterministic calls

# EXECUTION STEPS
1. Remove old generic `enforcement/enforcement/` scaffold
2. Create `enforcement/rule/` domain with all 7 DDD subfolders
3. Create `enforcement/violation/` domain with all 7 DDD subfolders
4. Implement RuleAggregate (Define, Evaluate, Deactivate) with events
5. Implement ViolationAggregate (Detect, Acknowledge, Resolve) with events
6. Create value objects: RuleId, RuleStatus, RuleScope, ViolationId, ViolationStatus, SourceReference
7. Create error classes, services, and specifications
8. Create README files for context and both domains
9. Update economic guard with enforcement E-RULES, D-RULES, C-CONSTRAINTS (C59-C66)

# OUTPUT FORMAT
- DDD domain structure under `src/domain/economic-system/enforcement/{rule,violation}/`
- Guard rules appended to `claude/guards/domain-aligned/economic.guard.md`
- Context README at `src/domain/economic-system/enforcement/README.md`
- Domain READMEs at `src/domain/economic-system/enforcement/{rule,violation}/README.md`

# VALIDATION CRITERIA
- Both domains have all 7 mandatory subfolders
- Correct namespaces: `Whycespace.Domain.EconomicSystem.Enforcement.{Rule,Violation}`
- Zero external dependencies beyond SharedKernel
- All ID value objects reject Guid.Empty
- All state changes via RaiseDomainEvent + Apply
- No financial type references (LedgerEntryAggregate, JournalAggregate, etc.)
- Guard rules cover all cross-domain invariants
- Event flow: RuleDefined → RuleEvaluated → ViolationDetected → ViolationAcknowledged → ViolationResolved
