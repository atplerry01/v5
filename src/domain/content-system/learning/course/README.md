# Course Domain

**Path:** `content-system/learning/course`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Course`

## Purpose
Owns the aggregate root of a course: title, owner, curriculum (ordered
module references), and lifecycle.

## Lifecycle
```
Draft → Published → Archived (terminal)
```

## Events
- `CourseDraftedEvent`
- `CourseModuleAttachedEvent`
- `CourseModuleDetachedEvent`
- `CoursePublishedEvent`
- `CourseArchivedEvent`

## Invariants
1. Owner required.
2. At least one module required to publish.
3. Archived courses are immutable.
4. Module refs unique per course.
