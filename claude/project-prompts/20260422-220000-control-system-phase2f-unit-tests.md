# TITLE
Phase 2F — Control-System Unit Tests: 35 Aggregate Test Files

# CONTEXT
Classification: control-system
Context: all 7 (access-control, audit, configuration, observability, scheduling, system-policy, system-reconciliation)
Domain: 35 bounded contexts (6 + 5 + 5 + 5 + 3 + 6 + 5)

Phase 2E (runtime wiring) was completed and audited. Phase 2F delivers unit test coverage for all 35 control-system aggregate classes.

# OBJECTIVE
Write one `{Domain}AggregateTests.cs` per bounded context under `tests/unit/control-system/{context}/{domain}/`. Each file must: use deterministic `Hex64` ID construction, test the factory method + all lifecycle transitions + guard violations, include `LoadFromHistory_RehydratesState` via private-constructor rehydration, and raise no determinism violations (no `Guid.NewGuid`, no system time).

# CONSTRAINTS
- Hex64(seed): `IdGen.Generate(seed).ToString("N") + IdGen.Generate(seed).ToString("N")` — 64-char lowercase hex
- `Activator.CreateInstance(typeof(XAggregate), nonPublic: true)` for private-constructor rehydration
- `Assert.ThrowsAny<Exception>` for all guard violation tests
- `aggregate.ClearDomainEvents()` before testing mutation methods
- No mocks, no DI, no infrastructure dependencies
- Gate 7 floor: ceil(35 × 0.5) = 18 minimum; delivering 35 for full coverage

# EXECUTION STEPS
1. For each of 35 BCs: read aggregate, read event types, read enum VOs
2. Write test file with [Fact] methods: factory raises event + sets state, each lifecycle method raises event + sets state, guard violations throw, LoadFromHistory rehydrates
3. Audit: no determinism violations, no stubs, all 35 have LoadFromHistory test, all use TestIdGenerator

# OUTPUT FORMAT
`tests/unit/control-system/{context}/{domain}/{Domain}AggregateTests.cs`

# VALIDATION CRITERIA
- 35 test files exist under `tests/unit/control-system/`
- All use `Hex64` (not `Guid.NewGuid`)
- All 35 contain `LoadFromHistory_RehydratesState`
- All 35 use `Activator.CreateInstance` for private-constructor rehydration
- Zero determinism violations (`grep -r "Guid.NewGuid" tests/unit/control-system/` → empty)
- Zero stubs (`grep -r "NotImplementedException\|TODO" tests/unit/control-system/` → empty)

# AUDIT RESULTS
- 35/35 test files created ✓
- 35/35 reference `Whycespace.Domain` ✓
- 35/35 reference `Whycespace.Tests.Shared` ✓
- 35/35 contain `LoadFromHistory_RehydratesState` ✓
- 35/35 use `Activator.CreateInstance` ✓
- 0 determinism violations ✓
- 0 stubs ✓
