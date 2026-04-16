# Phase 1.5 — Dependency Graph Audit

**STATUS: PASS** (post-Patch A, 2026-04-09)
**SCOPE: §2.7 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Post-Patch Result

```
=== DEPENDENCY GRAPH CHECK ===
Violations: 0
Status: PASS
```

Patch A applied to `scripts/dependency-check.sh`: the C5 "shared kernel
I/O" predicate now mirrors the §5.1.2 Step C-G comment-line exclusion.
The grep now drops lines whose first non-whitespace token after the
file:line prefix is `///`, `//`, `*`, `/*`, or `*/` before the I/O-marker
match runs. All 8 false-positive doc-comment hits are gone. Zero
production source files were touched.

---

## Original Findings (pre-Patch A, retained for audit history)

## Result of `bash scripts/dependency-check.sh`

```
=== DEPENDENCY GRAPH CHECK ===
Violations: 8
Status: FAIL
```

## Violations (verbatim, all 8)

```
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/health/PostgresPoolSnapshot.cs:12:/// <c>PendingAcquisitions</c> is not currently knowable: Npgsql does
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/health/PostgresPoolSnapshot.cs:16:/// future Npgsql version exposes it.
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/messaging/KafkaConsumerOptions.cs:7:/// previously incidental Confluent.Kafka / librdkafka defaults that
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs:7:/// incidental Npgsql library defaults. The host registers one
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs:8:/// <c>NpgsqlDataSource</c> per logical pool (currently <c>event-store</c>
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs:32:/// <c>NpgsqlConnectionStringBuilder</c>. Required.
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs:63:/// Default per-command timeout, in seconds, applied via the Npgsql
VIOLATION: shared kernel I/O leak: src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs:65:/// Default 30 (matches the Npgsql library default — kept explicit
```

## Findings

### Finding 1 — All 8 violations are XML doc-comment false positives

Every reported line is the **prefix `///`** (XML doc comment), not a `using` statement, type reference, or namespace alias. The mentioned tokens (`Npgsql`, `Confluent.Kafka`, `librdkafka`) are inside prose explanations of why the option records exist and what host-side concretes consume them.

Direct verification:

```
$ grep -RIn "^using.*Npgsql\|^using.*Confluent\|^using.*StackExchange\.Redis" src/shared/
(no output)
```

`Whycespace.Shared.csproj` has zero PackageReferences to `Npgsql`, `Confluent.Kafka`, `StackExchange.Redis`, `Minio`. The shared assembly literally cannot link against these libraries — the project would not build.

### Finding 2 — `scripts/dependency-check.sh` C5 predicate is over-broad

The C5 "shared kernel I/O" rule (lines 141–153 of `scripts/dependency-check.sh`) is a `grep -RIn` over `src/shared` that matches the pattern `Npgsql|Confluent\.Kafka|StackExchange\.Redis|Minio|System\.Net\.Http|System\.IO\.File` against **every line**, including comment lines. The §5.1.2 Step C-G hardening only added comment-line exclusion to the `R-DOM-01` / `DG-R5-HOST-DOMAIN-FORBIDDEN` predicates — not to C5.

### Pre-HC-6 baseline confirmation

5 of 8 violations are pre-existing (`KafkaConsumerOptions.cs` and `PostgresPoolOptions.cs` were authored under §5.2.1 PC-4 / PC-6 in 2026-04-08, before §5.2.4 began). Only the 2 violations in `PostgresPoolSnapshot.cs` were introduced by HC-6 — and they are explicit doc-comments documenting why `PendingAcquisitions` is unknowable from the Npgsql public API. None can be removed without losing essential design intent.

## Required remediation (one of)

**Option A — harden C5 predicate (recommended)**: Add the same comment-line exclusion to the C5 grep that §5.1.2 Step C-G applied to `R-DOM-01`. Concretely, change the C5 grep invocation in `scripts/dependency-check.sh` to exclude lines whose first non-whitespace token is `//` or `///` or `/*` or `*`. After the patch:
```
Violations: 0
Status: PASS
```

**Option B — remove the prose**: Strip every mention of "Npgsql" / "Confluent.Kafka" / "librdkafka" from XML doc comments in `src/shared/contracts/infrastructure/**`. This is invasive (8 doc-comments across 3 files), removes load-bearing design context, and is not recommended.

**Option A is the canonical fix** because it mirrors the §5.1.2 precedent and does not require touching shared/contracts files.

## Real architectural state (independent of script output)

| Edge                          | Status   | Verified by                      |
|-------------------------------|----------|----------------------------------|
| `src/domain/**` → external    | clean    | csproj has no external refs       |
| `src/shared/**` → external    | clean    | csproj has no Npgsql/Confluent/Redis/Minio refs |
| `src/host/**` → `src/domain/**` | absent | §5.1.1 D1 closure (verified)      |
| `src/projections/**` → `src/runtime/**` | absent | §5.1.1 verification-only       |
| `src/runtime/**` → `src/host/**` | absent  | HC-7/HC-8/MI-1/HC-9 all consume contracts via shared, not host |

The actual dependency graph is clean. The script reports FAIL due to a script-side over-broad regex.

## Status

**FAIL** as reported by the canonical script. The §1 closure rule (`Dependency graph clean OR explicitly waived`) is NOT satisfied because no waiver is currently recorded.

**Remediation path**: apply Option A (harden C5 predicate) and re-run. Expected post-fix output: `Violations: 0 / Status: PASS`. This is a 5-line script change with no source code touched.

This finding propagates to `phase1.5-final.audit.md` as one of two blockers.
