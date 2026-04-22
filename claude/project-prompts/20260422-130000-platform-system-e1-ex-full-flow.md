---
TITLE: platform-system — Full E1→EX Flow Implementation
CLASSIFICATION: platform-system
CONTEXT: all (command, envelope, event, routing, schema)
PHASES_IN_SCOPE: E1–E12 (E2/E5/E9/E11 partial — engines and schema modules exist, tests absent)
SOURCE: claude/project-topics/v3/platform-system.md
---

# CONTEXT

platform-system owns 5 domain groups (command, envelope, event, routing, schema) covering 20 leaf domains.
It defines the canonical interaction and communication language of the system — how systems talk, not what they mean.
It is protocol-level, structural, and contract-driven.

Prior to this prompt:
- 3 pre-existing build errors were fixed:
  - EventSchemaAggregate.Version shadowing AggregateRoot.Version → added `new` keyword + cast in lifecycle guard
  - EventDefinitionAggregate.Version same fix
  - DefineCommandCommand.Version type mismatch (string → int) + schema module mapper (added .ToString())
- All 20 domains have mandatory artifact folders (aggregate, error, event, value-object) ✅
- All 20 domains are missing optional folders (entity, service, specification) → needs gitkeep stubs
- Zero test files exist

# OBJECTIVE

Complete the full E1→EX flow for platform-system:
1. Add missing optional artifact folder stubs (entity, service, specification) for all 20 domains
2. Run E1XD audit checks against domain code
3. Implement E12 comprehensive test coverage (construction, equality, events, serialization, regression)

# CONSTRAINTS

- Anti-Drift ($5): No architecture changes, no new patterns, no renaming
- Domain purity (D-PURITY-01): No STJ attributes in domain code
- Fix only: structural issues and template misalignment
- No redesign of domain logic

# EXECUTION STEPS

1. Add entity/service/specification/.gitkeep for all 20 domains (60 files)

2. Run E1XD domain audit checks:
   - E1XD-DET-NORNG-01: No non-deterministic primitives
   - E1XD-DI-NONE-01: No DI imports
   - E1XD-STUB-NONE-01: No stubs/NotImplementedException
   - E1XD-ERR-NOBCL-01: No raw BCL throws
   - E1XD-VO-IMMUTABLE-01: No mutable setters in value-objects
   - D-PURITY-01: No STJ attributes in domain

3. Implement test files for each context:
   - PlatformSystemCommandTests.cs (command context: command-definition, command-envelope, command-metadata, command-routing)
   - PlatformSystemEnvelopeTests.cs (envelope context: header, message-envelope, metadata, payload)
   - PlatformSystemEventTests.cs (event context: event-definition, event-envelope, event-metadata, event-schema)
   - PlatformSystemRoutingTests.cs (routing context: dispatch-rule, route-definition, route-descriptor, route-resolution)
   - PlatformSystemSchemaTests.cs (schema context: contract, schema-definition, serialization, versioning)
   - PlatformSystemRegressionTests.cs (cross-cutting: replay consistency, lifecycle guards, type safety)

# OUTPUT FORMAT

- 60 .gitkeep stubs added
- 6 new test files under tests/unit/platform-system/
- No domain files modified beyond the pre-existing build error fixes
- Build clean, all tests pass

# VALIDATION CRITERIA

- All 20 domains pass E1XD-FOLDER-MUST-01 and E1XD-FOLDER-OPT-01
- Zero S0/S1 findings from E1XD domain audit
- All aggregates follow AggregateRoot inheritance and RaiseDomainEvent pattern
- Test coverage: construction (valid + invalid), equality, events raised, serialization stability
- Replay consistency: same inputs → equal values under repeated construction
