# TITLE
Guard System Normalization + Policy Enforcement (WBSM v3.5)

# CONTEXT
Upgrade guard/audit system to full WBSM v3.5 canonical compliance: domain-aligned guards, policy-first execution, workflow placement lock, engine purity, ES+CQRS lock.

# OBJECTIVE
Append-only normalization across guards and audits per strict spec.

# CONSTRAINTS
- Append-only; no deletion, rename, or move
- All rules must include Rule ID, Description, Enforcement
- WBSM v3.5 alignment
- Do not touch: clean-code, hash-determinism, policy-binding, prompt-container, composition-loader, program-composition

# EXECUTION STEPS
1. Create claude/guards/domain-aligned/ with economic, identity, governance, workflow, observability guards
2. Append normalization blocks to engine, runtime, kafka, projection guards
3. Append checks to determinism, runtime, engine, kafka, projection, policy audits

# OUTPUT FORMAT
Modified/new guard + audit files.

# VALIDATION CRITERIA
Determinism, runtime order, policy enforcement, dependency graph, projection, engine purity audits pass.

# CLASSIFICATION
system / governance / normalization
