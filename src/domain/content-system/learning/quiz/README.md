# Quiz Domain

**Path:** `content-system/learning/quiz`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Quiz`

## Purpose
Owns the definition and scoring of a quiz attached to a course.

## Lifecycle
```
Draft ──Add Questions──► Draft
Draft ──Publish────────► Published ──Score──► Published
Any   ──Archive────────► Archived (terminal)
```

## Events
- `QuizCreatedEvent`
- `QuizQuestionAddedEvent`
- `QuizPublishedEvent`
- `QuizScoredEvent`
- `QuizArchivedEvent`

## Invariants
1. Must belong to a course.
2. At least one question required to publish.
3. Only Draft accepts new questions.
4. Only Published may be scored.
5. Archived is terminal.
