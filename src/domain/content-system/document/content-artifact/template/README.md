# template

## Purpose

The `template` leaf owns reusable document templates — named, typed, optionally schema-bound templates with a draft/active/deprecated/archived lifecycle.

## Aggregate root

- `DocumentTemplateAggregate`

## Key value objects

- `DocumentTemplateId`
- `TemplateName`
- `TemplateType`
- `TemplateSchemaRef` (optional)
- `TemplateStatus` (Draft / Active / Deprecated / Archived)

## Key events

- `DocumentTemplateCreatedEvent`
- `DocumentTemplateUpdatedEvent`
- `DocumentTemplateActivatedEvent`
- `DocumentTemplateDeprecatedEvent`
- `DocumentTemplateArchivedEvent`

## Invariants and lifecycle rules

- A newly created template starts in `Draft`.
- `Update` is a no-op when name, type, and schema ref are unchanged.
- `Activate` rejects if already active, deprecated, or archived (only `Draft` → `Active`).
- `Deprecate` requires a non-empty reason and is rejected on archived or already-deprecated templates.
- Archived is terminal — double-archive is rejected.

## Owns

- Template identity, name, type, schema binding, status.
- Create / update / activate / deprecate / archive transitions.

## References

- `TemplateSchemaRef` — optional opaque reference to a schema definition; the template aggregate does not resolve schema semantics.

## Does not own

- The actual schema definition, template body, or rendering engine — those are outside the domain layer.
- Documents produced from the template — a document's relationship to its originating template is a downstream concern.
- Template versioning with lineage — if required, it would be modelled separately (analogous to `document/lifecycle/version`).
