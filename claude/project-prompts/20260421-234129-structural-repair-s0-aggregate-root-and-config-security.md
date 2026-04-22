# TITLE
S0 Structural Integrity Repair — AggregateRoot Enforcement & Configuration Security

# CONTEXT
Classification: structural-repair
Context: domain-system-wide / infrastructure
Domain: aggregate-root-enforcement, config-secret-safety

Pre-execution guard pass: All four canonical guards loaded (constitutional, runtime, domain, infrastructure).

S0 failures detected in post-execution audit:
1. 167 aggregate classes not inheriting AggregateRoot (166 files; 1 stub with no class).
2. Plaintext secrets in `appsettings.Development.json` and `docker-compose.yml`.

# OBJECTIVE
Fix all S0 audit failures. No feature work. No refactoring beyond structural repair.

1. AggregateRoot Enforcement: all `*Aggregate.cs` files under `src/domain/` must inherit `AggregateRoot`.
2. Configuration Security: no plaintext passwords or secret keys in committed config files.

# CONSTRAINTS
- Minimal changes only.
- Preserve business logic, event schemas, aggregate names.
- No new patterns, no domain redesign.
- Anti-drift: no architecture changes, no renaming.

# EXECUTION STEPS
1. Load canonical guards from `claude/guards/`.
2. Scan `src/domain/**/*Aggregate.cs` for missing `AggregateRoot` inheritance.
3. Categorize: 159 empty-shell aggregates (no events) vs 7 complex aggregates (with `_uncommittedEvents`).
4. Batch-fix 159 empty shells: add `using`, add `: AggregateRoot`, promote `private` methods to `protected override`.
5. Manual-fix 7 complex aggregates: remove `_uncommittedEvents` + `Version` field + `AddEvent()` + `GetUncommittedEvents()`; rewrite typed Apply methods as single `protected override void Apply(object domainEvent)` switch; replace `Apply(@e); AddEvent(@e); EnsureInvariants()` with `RaiseDomainEvent(@e)`.
6. Fix `appsettings.Development.json`: replace all literal credentials with `${VAR}` env substitutions.
7. Fix `docker-compose.yml`: replace 3 literal credentials (postgres-exporter password, pgadmin password, grafana password) with `${VAR}` substitutions.
8. Verify build succeeds with 0 errors.

# OUTPUT FORMAT
Structured result: aggregate count, files modified, config entries fixed, build status.

# VALIDATION CRITERIA
- Zero aggregates missing AggregateRoot (class-bearing files only).
- Zero `_uncommittedEvents` fields remaining.
- Zero literal passwords in committed config files.
- `dotnet build` exits 0 errors on domain, engines, runtime, and host projects.
