# Community Domain

**Path:** `content-system/engagement/community`
**Namespace:** `Whycespace.Domain.ContentSystem.Engagement.Community`

## Purpose
Represents a named community of actors with roles (Member, Moderator,
Owner). Owns membership lifecycle and role assignments.

## Lifecycle
```
Active ── Archive ──► Archived (terminal)
   │
   └── Join/Leave/AssignRole
```

## Events
- `CommunityCreatedEvent`
- `CommunityMemberJoinedEvent`
- `CommunityMemberLeftEvent`
- `CommunityRoleAssignedEvent`
- `CommunityArchivedEvent`

## Invariants
1. Name and owner required on creation.
2. At least one Owner required while active.
3. Archived is terminal.
4. Members cannot join twice.
