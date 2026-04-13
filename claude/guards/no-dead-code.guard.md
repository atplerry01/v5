# NO-DEAD-CODE GUARD (ND-R1)

**Status:** ACTIVE
**Severity baseline:** S0 = must remove immediately; S1 = should remove.
**Owner:** WBSM v3 structural integrity.

## Objective

Ensure the codebase contains only executable, referenced, and purposeful code.
Eliminate misleading, unused, or placeholder artifacts that introduce ambiguity.

---

## Core Rule

A file MUST NOT exist in the repository if it has no runtime or compile-time impact,
unless it is explicitly allowed under the Exceptions section.

---

## Definitions

### Dead Code

Code is considered DEAD if ANY of the following are true:

1. It is not referenced anywhere in the codebase
2. It is not part of the build output
3. It is not used by runtime execution (API → Runtime → Engine → Domain → Projection)
4. It is not used by tests
5. It exists only as:

   * a placeholder
   * a redirect stub
   * commented-out logic
   * legacy artifact

---

## Prohibited Patterns

The following MUST NOT exist:

### 1. Redirect Stubs

Example:

```csharp
// Moved to ...
public class X {}
```

### 2. Empty Classes

```csharp
public class TodoService {}
```

### 3. Unused Interfaces / Implementations

* Interfaces with zero callers
* Implementations not registered or invoked

### 4. Duplicate Execution Paths

* Old consumers alongside new ones
* Parallel patterns doing the same job

### 5. Commented-Out Code Blocks

```csharp
// var x = ...
```

---

## Allowed Exceptions

The following are allowed:

### 1. Structural Placeholders

Used to preserve folder structure:

* `.gitkeep`

### 2. Scaffolding for Future Layers

ONLY if empty and intentional:

* `T3I/`
* `T4A/`

Must contain:

* no logic
* no fake implementations

### 3. Documentation (Outside Runtime Path)

* `/docs/`
* `/claude/`

NOT allowed inside:

* `/src/`

### 4. Contracts in Active Use

* DTOs, Commands, Queries even if indirectly referenced

---

## Enforcement Rules

### R1 — Reference Check

Every class must have at least one of:

* direct reference
* DI registration
* runtime invocation
* test usage

### R2 — Registration Check

If a class is meant to execute:

* it MUST be registered (DI / engine registry / workflow registry)

### R3 — Projection Consumption

* No custom consumers if a generic worker exists
* All projections must be reachable via registry

### R4 — Single Pattern Rule

* Only ONE valid implementation pattern per concern

---

## Violation Severity

| Severity | Description                                      |
| -------- | ------------------------------------------------ |
| S0       | Dead code affecting runtime clarity or execution |
| S1       | Unused but harmless code                         |
| S2       | Cosmetic / formatting                            |

---

## Action on Violation

* S0 → MUST be removed immediately
* S1 → SHOULD be removed
* S2 → optional cleanup

---

## Example (Correct)

✔ Used handler registered in engine registry
✔ Projection handler registered in projection registry

## Example (Violation)

✘ Old event consumer not used
✘ Empty service class
✘ Stub file with comment "moved to ..."

---

## Canonical Principle

> If a developer cannot trace a file to execution, it must not exist.

---

## Scope

Applies to:

* src/domain
* src/engines
* src/runtime
* src/systems
* src/projections
* src/platform
* src/shared

---

## Summary

Dead code is not neutral—it creates false architecture.

This guard ensures:

* clarity
* determinism
* maintainability
* correct developer understanding
