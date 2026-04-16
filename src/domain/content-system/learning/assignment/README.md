# Assignment Domain

**Path:** `content-system/learning/assignment`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Assignment`

## Purpose
Owns a course assignment — creation, publication, submission receipt,
grading, and closure.

## Lifecycle
```
Draft → Published → (repeated Receive/Grade) → Closed (terminal)
```

## Events
- `AssignmentCreatedEvent`
- `AssignmentPublishedEvent`
- `AssignmentSubmissionReceivedEvent`
- `AssignmentSubmissionGradedEvent`
- `AssignmentClosedEvent`

## Invariants
1. Belongs to a course.
2. `MaxGrade > 0`.
3. Only Draft may be published.
4. Only Published accepts submissions.
5. Submission id unique per assignment; each may be graded at most once.
6. Closed is terminal.
