# TITLE
phase1-gate-S1 — Eliminate policy bypass on TodoController.Create

# CONTEXT
Phase 1 Hardening Pass, Task 8 (Policy Coverage). VERDICT.md Drift #4
identified that `TodoController.Create` routes through `ITodoIntentHandler`
which does not flow through `RuntimeControlPlane` middleware, while
`Update` and `Complete` correctly use `ISystemIntentDispatcher.DispatchAsync`.
This is a $8 (Policy) violation: not all commands traverse WHYCEPOLICY.
Severity: S0.

Classification: infrastructure / phase1-hardening / policy-binding
Domain: operational/sandbox/todo (call site); shared dispatcher contract

# OBJECTIVE
Make `POST /api/todo/create` flow through `ISystemIntentDispatcher` so the
policy envelope, guards, event store, outbox, and chain anchor all execute
on the same path used by Update/Complete. After the fix, every command
issued by the API surface MUST go through the dispatcher.

# CONSTRAINTS
- $5 anti-drift: no architecture changes, no new patterns, no renames.
- $7 layer purity: controller stays in platform/api; no domain leakage.
- $9 determinism: no `Guid.NewGuid`. Use `IIdGenerator` with a stable seed.
- $8 policy: command MUST traverse the dispatcher path (no bypass).
- Preserve API contract: request shape (Title, Description?, UserId) and
  response shape (`{status, todoId}`) remain identical.
- Description and UserId are NOT part of CreateTodoCommand today; preserve
  the API field but acknowledge they are unused downstream — do not invent
  new command fields in this prompt.

# EXECUTION STEPS
1. Inject `IIdGenerator` and `IClock` into TodoController.
2. In `Create`: derive deterministic aggregate id from
   `"{UserId}:{Title}:{clock.UtcNow.Ticks}"`.
3. Build `CreateTodoCommand(id, title)`.
4. Dispatch via `_dispatcher.DispatchAsync(cmd, TodoRoute)`.
5. Return `{status: "created", todoId: id}` on success;
   `BadRequest(result.Error)` on failure.
6. Remove `_intentHandler` field, constructor parameter, and unused
   `Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo` using.
   (DI registration of ITodoIntentHandler is left in place; cleanup is
   out of scope for this prompt.)

# OUTPUT FORMAT
Single-file diff to TodoController.cs. No other files touched.

# VALIDATION CRITERIA
- `dotnet build` succeeds.
- TodoController has zero references to `ITodoIntentHandler`.
- Create path produces a `PolicyEvaluatedEvent` row (verified next gate run).
- No `Guid.NewGuid` introduced.
