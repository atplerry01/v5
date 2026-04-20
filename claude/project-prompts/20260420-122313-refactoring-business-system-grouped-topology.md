---
timestamp: 20260420-122313
classification: refactoring
context: business-system
topic: grouped-topology-truth-linked-canonical-rebuild
---

# TITLE
Business-System Topology & Classification Refactor — Grouped Canonical Rebuild (Truth-Linked Doctrine)

# CONTEXT
WBSM v3 canonical execution rule set; DS-R3 / DS-R3a permit 4-level grouped topology `{classification}/{context}/{domain-group}/{domain}/` per-context.
Current `src/domain/business-system/**` contains 15 contexts and ~146 leaf domains; most of this is misclassified truth that has accumulated under business-system but actually represents content, economic, structural/reference, operational, or infrastructure truth. The repository has a locked 4-layer guard model and requires truth-correct placement over convenience.

# OBJECTIVE
Refactor `src/domain/business-system/` into the **grouped canonical target structure** containing exactly 8 contexts:
`agreement`, `customer`, `entitlement`, `offering`, `order`, `pricing`, `provider`, `service`.

Each context adopts 4-level grouped topology with explicit domain-groups and leaves per the target model. Non-business-true contexts are removed from `business-system` and recorded as follow-ups for cross-system placement.

# CONSTRAINTS
- Apply truth-linked doctrine strictly: business-system = business meaning truth only.
- Do not preserve legacy contexts by default; prefer deletion.
- Obey canonical 4-layer guard model (`constitutional`, `runtime`, `domain`, `infrastructure`).
- Adhere to domain guard rules: DS-R3 / DS-R3a (per-context grouping choice), DS-R6 (namespace = folder), DS-R8 (`DomainRoute` remains 3-tuple).
- Preserve git history via `git mv` where possible.
- No architectural drift beyond the target model; no net-new design outside of the canonical target.
- Every target leaf must have the 7 canonical artifact subfolders (`aggregate`, `entity`, `error`, `event`, `service`, `specification`, `value-object`) with `.gitkeep` placeholders for empty folders, plus a `README.md`.
- `agreement/change-control/amendment` and `order/order-change/amendment` must remain semantically distinct.
- `offering/service-offering` and `service/service-definition` must remain semantically distinct.
- `customer/account` means business customer account only — not identity/auth/ledger/platform account.

# EXECUTION STEPS
1. Discovery — enumerate current contexts and leaf domains under `src/domain/business-system/`.
2. Mapping — build a per-current-domain action table (`keep | move | merge | delete | reclassify`) against the target.
3. Scaffold — create target grouped tree with 7 artifact subfolders per leaf and stub READMEs.
4. Migrate — `git mv` existing business-true domains to new grouped paths; update C# namespaces to include domain-group segment (per DS-R6 with 4-level).
5. Remove — `git rm -r` misclassified contexts and domains; log each removed domain with its correct target system.
6. Structural check — no legacy contexts left; consistent grouping per context; canonical names.
7. Produce final report.

# OUTPUT FORMAT
Final report with sections:
1. CURRENT → TARGET MAPPING
2. FILES / FOLDERS CREATED
3. FILES / FOLDERS MOVED
4. FILES / FOLDERS DELETED
5. FINAL BUSINESS-SYSTEM TREE
6. REMOVED / RECLASSIFIED NON-BUSINESS DOMAINS
7. OPEN FOLLOW-UPS OUTSIDE BUSINESS-SYSTEM

# VALIDATION CRITERIA
- `src/domain/business-system/` contains exactly 8 context folders matching target.
- Each context uses consistent grouped topology (no flat + grouped mixing within a context).
- Every leaf has 7 canonical artifact folders + `README.md`.
- No domain folder reclassified as non-business remains inside business-system.
- Moved C# namespaces match new 4-level folder path.
- No `-system` suffix leaks outside `src/domain/`.
- Post-execution audit sweep is clean (or any drift is captured in `claude/new-rules/`).
