````markdown
# Claude-Ready Prompt — Content-System Canonical Realignment
# Style: writing-code / implementation-control prompt
# Mode: strict WBSM v3.5 / Whycespace / no-drift
# Target: refactor and realign existing content-system to the new canonical tree

You are working inside the Whycespace WBSM v3.5 codebase.

You must perform a **strict content-system canonical realignment** against the newly defined target model.

This is a **controlled refactor + gap-identification + implementation-checklist generation task**.
Do not redesign the project.
Do not introduce drift.
Do not invent alternate structures.
Do not apply “best effort” simplifications that violate the locked target tree.
Honor existing project doctrine, domain-structure rules, event-sourcing rules, determinism rules, README standards, and canonical boundary rules.

---

## attached input
The current content-system snapshot is attached and must be treated as the primary inspection source for this task.
Validate against the actual attached tree/files, not assumptions.

## do not skip real mapping
Do not give a conceptual answer only.
You must map real existing domains from the attached content-system to the target canonical tree.

## if code changes are not requested
If this run is analysis-only, do not modify files yet.
Produce the mapping, decisions, and implementation checklist first.



## 0. constitutional working rules

You must follow these rules throughout:

- `content-system` is the authoritative owner of content truth.
- Other systems may consume content truth, but they do not own content state.
- Use the canonical domain form:
  - `CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN`
- Domain path rule:
  - under `src/domain`, parent classification folders use `*-system`
  - therefore the target root remains:
    - `src/domain/content-system/...`
- Do not change other classifications.
- Do not touch runtime / engines / platform / infrastructure unless required by documentation or mapping output for explanation.
- This task is focused on:
  - current-state mapping
  - canonical drift detection
  - overlap detection
  - gap detection
  - strict implementation checklist generation
  - optional README and structure planning
- If code moves are made, they must be justified by the canonical mapping matrix.
- Preserve event-sourced and deterministic design.
- Preserve canonical 7 artifact folders at leaf domains where required by project standard.
- No external dependencies.
- No weakening of domain boundaries.
- No business-use-case drift into content-system.

---

## 1. task objective

Use the **current built content-system** as the source input.

Map the existing domains against this **locked target canonical tree**:

```text
content-system
- document
  - intake
    - upload
    - import
  - descriptor
    - metadata
  - core-object
    - file
    - document
    - attachment
    - record
    - bundle
    - template
  - integrity-provenance
    - integrity
    - provenance
  - lifecycle-change
    - processing
    - version
    - review
    - publication
  - representation
    - preview
    - export
  - governance
    - classification
    - retention
    - moderation

- media
  - intake
    - ingest
  - descriptor
    - metadata
  - core-object
    - asset
    - rendition
    - artwork
    - transcript
    - subtitle
    - preview
  - technical-processing
    - processing
    - quality
    - integrity
  - composition-catalog
    - package
    - sequence
  - lifecycle-change
    - version
  - rights-publication
    - rights
    - publication
  - safety-governance
    - moderation
    - accessibility

- streaming
  - stream-core
    - stream
    - manifest
    - availability
    - channel
  - playback-consumption
    - session
    - progress
    - replay
  - live-streaming
    - broadcast
    - ingest-session
    - archive
  - delivery-governance
    - access
    - entitlement-hook
    - moderation
    - observability

- shared
  - relationship
    - relationship
  - localization
    - localization
  - provenance-evidence
    - evidence
````

This tree is the target alignment model for this task.

---

## 2. known current-state assumptions to validate

The current content-system is expected to contain a shape approximately like this:

### document context

* `content-artifact/document`
* `content-artifact/file`
* `content-artifact/bundle`
* `content-artifact/record`
* `content-artifact/template`
* `descriptor/metadata`
* `lifecycle/upload`
* `lifecycle/processing`
* `lifecycle/retention`
* `lifecycle/version`

### media context

* `content-artifact/asset`
* `content-artifact/audio`
* `content-artifact/video`
* `content-artifact/image`
* `content-artifact/media-file`
* `companion-artifact/subtitle`
* `companion-artifact/transcript`
* `descriptor/metadata`
* `lifecycle/upload`
* `lifecycle/processing`
* `lifecycle/version`

### streaming context

* `stream-core/stream`
* `stream-core/live-stream`
* `stream-core/channel`
* `stream-core/stream-session`
* `delivery-artifact/manifest`
* `delivery-artifact/segment`
* `delivery-artifact/playback`
* `control/access`
* `persistence-and-observability/recording`
* `persistence-and-observability/metrics`

You must validate against the actual repo state rather than assuming this blindly.

---

## 3. required outputs

You must produce all of the following.

### Output A — current-to-canonical mapping matrix

Create a precise mapping report from:

* current path
* current ownership intent
* target canonical path
* action type:

  * preserve
  * rename
  * move
  * split
  * merge
  * retire
  * add-new
* rationale
* risk notes

This must be exhaustive for all current content-system domains.

### Output B — drift / overlap / gap report

Produce a strict report identifying:

* structural drift
* doctrinal drift
* overlapping ownership
* aggregate ambiguity
* missing first-class domains
* canonical omissions that should be restored
* questionable current domains that may need retirement or demotion

### Output C — recommended canonical decisions

Explicitly decide and justify:

* keep `document/descriptor/metadata` as first-class domain
* keep `media/lifecycle-change/version` as first-class domain
* merge or preserve `audio`, `video`, `image`
* preserve or fold `media-file`
* preserve or retire `segment`
* preserve `channel` or move it out
* split `recording` into `archive` and possibly `replay`
* keep `access` distinct from `entitlement-hook`

You must not leave these vague.

### Output D — strict Phase 2.6-style implementation checklist

Generate a controlled execution checklist with ordered gates using this style:

* `P2.6.CS.0`
* `P2.6.CS.1`
* `P2.6.CS.2`
* etc.

Each gate must include:

* purpose
* scope
* actions
* definition of done
* validation
* exit criteria

It must be implementation-usable, not just advisory.

### Output E — README/update plan

Produce a README rewrite plan covering:

* root content-system README
* each context README
* each domain-group README
* each leaf README if required by the current standard

Each README plan must state:

* purpose
* owns
* does not own
* lifecycle/state
* event examples
* upstream/downstream references

### Output F — final change strategy

Provide a practical migration strategy:

* what can be remapped first
* what should be split later
* what can be preserved temporarily
* what should be introduced as new scaffolds
* what must be retired only after replacement is present

---

## 4. mandatory analysis rules

When analyzing current domains, apply these domain-boundary rules:

### document context owns

* document/file/record/template/bundle truth
* document-specific metadata
* upload/import/processing/version/review/publication
* retention/classification/moderation
* preview/export
* integrity/provenance

### media context owns

* asset truth
* rendition/artwork/transcript/subtitle/preview
* media metadata
* processing/quality/integrity
* package/sequence
* rights/publication
* moderation/accessibility
* version lineage

### streaming context owns

* stream identity
* manifest/availability/channel
* playback session/progress/replay
* live broadcast/ingest-session/archive
* access + entitlement hook
* moderation/observability

### shared owns only truly cross-context content truth

* relationship graph
* localization variants
* evidence linkage

Do not let shared become a dumping ground.

---

## 5. critical decision rules

Apply these unless actual code proves a stronger reason otherwise:

* Keep `document/descriptor/metadata` as a first-class domain.
* Keep `media/lifecycle-change/version` as a first-class domain.
* Merge `audio`, `video`, `image` into `asset` unless they have truly independent aggregate behavior, invariants, and lifecycle.
* Keep `media-file` only if it has independent durable lifecycle/integrity semantics.
* Keep `channel` only if it is authoritative streaming publication/container truth.
* Treat `segment` as infrastructure unless it is clearly durable business-relevant domain truth.
* Split `recording` into `archive`, and add `replay` if replay is publishable truth distinct from archive.
* Keep `access` distinct from `entitlement-hook`; they are not the same concern.

---

## 6. output format requirements

Return the result in **writing-code style** using markdown code blocks.

Use this exact high-level format:

### 1. `CURRENT_TO_CANONICAL_MAPPING.md`

A code block with the full mapping matrix.

### 2. `DRIFT_OVERLAP_GAP_REPORT.md`

A code block with the drift / overlap / gap report.

### 3. `CANONICAL_DECISIONS.md`

A code block with the final architectural decisions.

### 4. `PHASE_2_6_CONTENT_SYSTEM_CHECKLIST.md`

A code block with the strict implementation checklist.

### 5. `README_UPDATE_PLAN.md`

A code block with the README rewrite/update plan.

### 6. `MIGRATION_STRATEGY.md`

A code block with the migration rollout strategy.

Do not output prose outside those code blocks except for a very short header and final verdict.



---

## 7. execution behavior

Be concrete.
Be exhaustive.
Be strict.
Prefer exactness over elegance.
Do not soften findings.
Do not hide ambiguity — isolate and resolve it.
Do not rewrite unrelated domains.
Do not invent cross-system features inside content-system.
Do not drop existing real domains unless retirement is explicitly justified.

If something in the current repo contradicts this target tree:

* report it
* map it
* classify it as preserve / split / merge / retire / defer
* explain why


### 7. `DEFERRED_ITEMS.md`
A code block listing:
- domains to defer
- why they are deferred
- what prerequisite decision or implementation is required first
- 
---

## 8. preferred inspection sequence

Use this review order:

1. inspect current content-system root
2. inspect document context
3. inspect media context
4. inspect streaming context
5. inspect missing shared context
6. compare against canonical target
7. produce mapping matrix
8. produce drift/overlap/gap report
9. produce decisions
10. produce strict implementation checklist
11. produce README plan
12. produce migration strategy

---

## 9. quality bar

The result is only acceptable if:

* every current domain is accounted for
* every target canonical domain is accounted for
* all major drift is identified
* overlaps are named explicitly
* missing domains are listed explicitly
* decisions are concrete
* the checklist is execution-grade
* README plan is specific
* migration order is safe and practical

If needed, be opinionated — but remain inside the locked doctrine and target tree.

Start now.

