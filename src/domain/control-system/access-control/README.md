# control-system / access-control

**Classification:** control-system  
**Context:** access-control

## Purpose
Owns the structural access control model: who may act (authorization records), what they may do (permissions), and how capabilities are grouped (roles). No session management — sessions carry identity lifecycle semantics and are excluded from this context.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `authorization` | Subject authorization records — the binding of a subject to a resolved permission set |
| `permission` | Discrete permission definitions (capability + scope + action mask) |
| `role` | Named role aggregating a set of permissions |

## Removed (violations)
- `session` — identity/session lifecycle semantics; excluded per classification rules

## Does Not Own
- Policy definition or evaluation (→ system-policy)
- Identity management or user profiles (deferred)
- Session management (deferred — identity lifecycle concern)
- Audit of access events (→ audit)

## Inputs
- Role assignment from administrative bootstrap
- Permission definitions from system bootstrap

## Outputs
- `AuthorizationGrantedEvent`
- `AuthorizationRevokedEvent`
- `RoleDefinedEvent`
- `PermissionDefinedEvent`

## Invariants
- A subject's authorization is the union of permissions across assigned roles
- Permissions are immutable once defined; updates create new versions
- Circular role inheritance is forbidden

## Dependencies
- `core-system/identifier` — subject, role, permission, and authorization IDs
- `core-system/temporal` — authorization validity windows
