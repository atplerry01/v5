# Prompt Container Guard

## Purpose

Enforce canonical prompt formatting across all AI-assisted prompts used in the WBSM v3 system. Every prompt must use the markdown container format, declare mandatory sections, avoid broken fencing, support batch execution, and be registered in the prompt registry.

## Scope

All prompt files (`.prompt.md`, `.prompt.json`, or prompt templates) across the repository, including `claude/` directory prompts, CI prompts, and any prompt used for code generation, auditing, or governance. Evaluated at CI and prompt review.

## Rules

1. **MARKDOWN CONTAINER FORMAT** — All prompts must use the standard markdown container structure. Each prompt is a self-contained markdown document with clearly delineated sections using level-2 headings (`##`). No free-form text prompts. No inline prompt strings embedded in code. Every prompt is a file.

2. **MANDATORY SECTIONS** — Every prompt must declare these five sections:
   - `## Role` — Defines the AI's persona, expertise, and constraints for this prompt.
   - `## Objective` — States the specific goal of the prompt in one to three sentences.
   - `## Rules` — Numbered list of behavioral rules the AI must follow during execution.
   - `## Output` — Defines the expected output format, structure, and delivery method.
   - `## Failure` — Defines what constitutes failure, how to detect it, and what to do on failure.
   Missing any section makes the prompt non-compliant.

3. **NO BROKEN NESTED CODE FENCING** — Prompts that contain code examples must use proper fencing. Triple backticks inside a prompt must not break the outer markdown structure. Use different fence lengths (````` vs ```) or indent-based code blocks when nesting. A prompt with broken fencing is unparseable and therefore invalid.

4. **PROMPTS ARE BATCH-SAFE** — Every prompt must be executable in batch mode (non-interactive). Prompts must not require mid-execution user input, confirmations, or interactive decisions. All parameters must be declared upfront in a `## Parameters` section (optional but required if the prompt takes input). Batch-safe means: given inputs, the prompt runs to completion autonomously.

5. **PROMPTS REGISTERED IN PROMPT REGISTRY** — Every prompt file must have an entry in `prompt.registry.json` (or equivalent registry file). The registry entry includes: prompt ID, file path, category, version, and last-validated date. Unregistered prompts are not executable by CI or automated systems.

6. **PROMPT VERSIONING** — Each prompt must declare its version in a YAML frontmatter block or metadata section. Version follows semver: `major.minor.patch`. Breaking changes to prompt structure increment major. Output format changes increment minor. Clarifications increment patch.

7. **PROMPT CATEGORIZATION** — Prompts must be categorized:
   - **audit**: Prompts that validate code or architecture.
   - **generate**: Prompts that produce code, configs, or artifacts.
   - **review**: Prompts that evaluate PRs, diffs, or changes.
   - **enforce**: Prompts that check compliance against guards.
   - **report**: Prompts that produce summary or status reports.
   The category must be declared in the prompt metadata and registry entry.

8. **NO PROMPT INJECTION VECTORS** — Prompts must not contain user-controllable interpolation without sanitization. If a prompt accepts parameters, parameter values must be enclosed in delimited blocks (e.g., `<parameter>value</parameter>`) to prevent prompt injection. No raw string concatenation of user input into prompt text.

9. **DETERMINISTIC OUTPUT SPECIFICATION** — The `## Output` section must define the exact output format (JSON schema, markdown template, structured report format). Outputs must be machine-parseable when consumed by CI. Free-form prose output is permitted only for human-targeted prompts explicitly marked as such.

10. **PROMPT DEPENDENCY DECLARATION** — If a prompt depends on output from another prompt (chained execution), the dependency must be declared in a `## Dependencies` section. The dependency graph must be acyclic. Circular prompt dependencies are forbidden.

11. **PROMPT IDEMPOTENCY** — Prompts must be idempotent: running the same prompt with the same inputs on the same codebase state must produce equivalent output. Non-deterministic prompts (e.g., creative generation) must be explicitly marked as `idempotent: false` in metadata.

12. **MAXIMUM PROMPT LENGTH** — Individual prompts must not exceed 4000 words. Prompts exceeding this limit must be decomposed into chained sub-prompts with explicit dependency declarations. Overly long prompts degrade AI performance and are harder to audit.

13. **WRITING BLOCK REQUIRED FOR LONG PROMPTS** — Prompts that generate or modify more than 5 files, or produce output exceeding 2000 lines, must include a `## Writing Block` section. The writing block declares: target files, expected changes, rollback strategy, and validation criteria. This ensures large-scale prompt executions are auditable and reversible.

14. **NO BROKEN CONTAINERS (CRITICAL)** — A prompt container must be syntactically complete. Every opened section must be closed. Every code fence must be balanced. Every parameter placeholder must have a corresponding declaration. A broken container is a prompt that cannot be parsed to completion. Broken containers must fail CI immediately with S0 severity. No partial prompt execution is permitted.

15. **PROMPT MUST DECLARE EXECUTION MODE** — Every prompt must declare its execution mode in metadata or frontmatter:
    - `mode: autonomous` — prompt runs to completion without human intervention
    - `mode: supervised` — prompt pauses at checkpoints for human review
    - `mode: dry-run` — prompt simulates execution and reports what would change
    Prompts without a declared execution mode default to `supervised` and must be flagged for metadata completion.

---

## WBSM v3 GLOBAL ENFORCEMENT

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Check Procedure

1. Enumerate all prompt files matching `*.prompt.md`, `*.prompt.json`, or files in prompt directories.
2. For each prompt file, verify presence of all five mandatory sections: Role, Objective, Rules, Output, Failure.
3. Parse markdown fencing and verify no broken nested code blocks (fence depth tracking).
4. Verify no prompt requires mid-execution user interaction (scan for interactive markers).
5. Verify each prompt has an entry in `prompt.registry.json`.
6. Verify version declaration in frontmatter or metadata for each prompt.
7. Verify category assignment for each prompt.
8. Scan for raw string interpolation patterns (e.g., `${userInput}`, `{0}`) without delimiters.
9. Verify `## Output` section defines structured format (JSON schema, template, or format spec).
10. Build prompt dependency graph and check for cycles.
11. Verify prompt word count does not exceed 4000 words.
12. Cross-reference registry entries against actual prompt files (detect orphaned registrations or unregistered prompts).

## Pass Criteria

- All prompts use markdown container format.
- All prompts have all five mandatory sections.
- No broken code fencing in any prompt.
- All prompts are batch-safe (no interactive requirements).
- All prompts registered in prompt registry.
- All prompts have version declarations.
- All prompts are categorized.
- No prompt injection vectors detected.
- All output specifications are structured.
- Prompt dependency graph is acyclic.
- All prompts under 4000 words.

## Fail Criteria

- Prompt missing mandatory section (Role, Objective, Rules, Output, or Failure).
- Broken nested code fencing.
- Prompt requires interactive input mid-execution.
- Prompt not registered in prompt registry.
- Missing version declaration.
- Missing category assignment.
- Raw user input interpolation without sanitization.
- Unstructured output specification for CI-consumed prompt.
- Circular prompt dependency.
- Prompt exceeds 4000 words without decomposition.
- Orphaned registry entry (prompt file deleted but registry entry remains).

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Prompt injection vector | `${userInput}` concatenated into role section |
| **S0 — CRITICAL** | Missing mandatory section | Prompt without `## Failure` section |
| **S1 — HIGH** | Unregistered prompt | Prompt file exists but not in registry |
| **S1 — HIGH** | Broken code fencing | Triple backticks break outer markdown |
| **S1 — HIGH** | Interactive prompt in CI pipeline | Prompt asks "Continue? (y/n)" mid-execution |
| **S2 — MEDIUM** | Missing version | No version in frontmatter or metadata |
| **S2 — MEDIUM** | Circular dependency | Prompt A depends on B, B depends on A |
| **S2 — MEDIUM** | Unstructured output | Output section says "describe the findings" |
| **S3 — LOW** | Missing category | Prompt without category assignment |
| **S3 — LOW** | Prompt over 4000 words | Long prompt without decomposition |
| **S3 — LOW** | Orphaned registry entry | Registry points to deleted prompt file |

## Enforcement Action

- **S0**: Block merge. Fail CI. Prompt must be fixed or removed immediately.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
PROMPT_CONTAINER_GUARD_VIOLATION:
  prompt: <prompt file path>
  registry_id: <registry ID if applicable>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  section: <which section is affected>
  expected: <correct format>
  actual: <detected issue>
  remediation: <fix instruction>
```
