# Module Domain

**Path:** `content-system/learning/module`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Module`

## Purpose
A module inside a course: ordered container for lessons.

## Lifecycle
```
Draft → Published → Archived (terminal)
```

## Events
- `ModuleCreatedEvent`
- `ModuleReorderedEvent`
- `ModulePublishedEvent`
- `ModuleArchivedEvent`

## Invariants
1. Must reference a course.
2. Order non-negative.
3. Archived is terminal.
