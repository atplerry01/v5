# TITLE
Enforcement & Compliance Cross-Domain Rules Lock

# CONTEXT
Classification: economic-system
Context: enforcement, compliance (cross-domain)
Domain: rule, violation, audit
Phase: Guard hardening

# OBJECTIVE
Replace context-specific enforcement and compliance guard sections with a unified cross-domain rule set (E1-E10, D66-D74, C51-C60) that governs both control layers under a single coherent framework.

# CONSTRAINTS
- Reconciliation cross-domain rules D63-D65, C48-C50 preserved
- D66 onward renumbered for enforcement + compliance unified section
- C51 onward renumbered for enforcement + compliance unified section
- No domain code changes — guard-only update

# EXECUTION STEPS
1. Trim reconciliation cross-domain section to D63-D65 and C48-C50
2. Remove old compliance-specific section (D68-D73, C53-C58)
3. Remove old enforcement-specific section (D74-D80, C59-C66)
4. Add unified ENFORCEMENT & COMPLIANCE CROSS-DOMAIN RULES section
5. Lock E1-E10 (cross-domain rules), D66-D74 (guard extensions), C51-C60 (audit extensions)

# OUTPUT FORMAT
Updated `claude/guards/domain-aligned/economic.guard.md`

# VALIDATION CRITERIA
- E1-E10 cover: rule foundation, source traceability, no financial mutation, terminal resolution, reviewability, immutable evidence, unique rule identity, violation-rule binding, audit-source binding, evidence-not-policy
- D66-D74 map to structural checks enforceable against code
- C51-C60 are violation codes with severity levels
- Check procedure covers both enforcement and compliance contexts
- No orphan D-rules or C-codes from prior sections remain
