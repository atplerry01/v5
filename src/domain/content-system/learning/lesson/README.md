# Lesson Domain

**Path:** `content-system/learning/lesson`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Lesson`

## Purpose
Owns a single teachable unit inside a module: body content and lifecycle.

## Lifecycle
```
Draft → Published → Archived (terminal)
```

## Events
- `LessonCreatedEvent`
- `LessonUpdatedEvent`
- `LessonPublishedEvent`
- `LessonArchivedEvent`

## Invariants
1. Must belong to a module.
2. Body non-empty and bounded.
3. Archived is terminal.
