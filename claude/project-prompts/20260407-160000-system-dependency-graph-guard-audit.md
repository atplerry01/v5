---
classification: system
context: governance
domain: dependency-control
priority: S0
mode: ENFORCEMENT
scope: repository
---

# TITLE
WHYCESPACE — Dependency Graph Guard + Audit (Full Enforcement)

# CONTEXT
Enforce strict, provable layer dependency rules across the repository so that
illegal references are impossible to introduce, violations are detected
automatically, and CI fails on any violation.

# OBJECTIVE
- Lock canonical layer order: platform → systems → runtime → engines → domain → shared
- Create dependency-graph guard + audit
- Provide automated dependency-check script
- Wire CI enforcement
- Produce final dependency report

# CONSTRAINTS
- Layer purity ($7), Anti-drift ($5), File system scope ($6)
- No architectural rewrites without authorization
- Guard + audit are non-negotiable, loaded fresh each execution

# EXECUTION STEPS
1. Create /claude/guards/dependency-graph.guard.md (R1–R7)
2. Create /claude/audits/dependency-graph.audit.md
3. Scan all .csproj for forbidden upward/cross references
4. Scan using statements for layer leakage
5. Adapter leakage check (adapters only in platform/host/adapters or infrastructure)
6. Shared kernel validation (primitives/contracts/interfaces only)
7. Create /scripts/dependency-check.sh
8. Wire CI: dotnet build + bash scripts/dependency-check.sh
9. Emit final report

# OUTPUT FORMAT
- Dependency graph
- Violations list (file:line → description)
- Fixes applied
- Final PASS/FAIL

# VALIDATION CRITERIA
LOCKED only if all R1–R7 PASS, no illegal refs, no cycles, CI enforces, audit
file shows FULL PASS.