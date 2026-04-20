# 04 — API + Projection + Tests

## API layer

Reference: [src/platform/api/controllers/economic/](../../../src/platform/api/controllers/economic/)

### Controller layout

```
src/platform/api/controllers/{classification}/
  {context}/
    {domain}/
      {Domain}Controller.cs
```

Naming uses raw classification (no `-system` suffix) per `domain.guard.md` DS-R2.

### Controller pattern

Per behavioral rule 5 (COMMANDS FLOW THROUGH RUNTIME ONLY):

```csharp
[ApiController]
[Route("api/economic/capital/account")]
public sealed class AccountController(ISystemIntentDispatcher dispatcher) : ControllerBase
{
    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] OpenAccountRequestModel request)
    {
        var command = new OpenCapitalAccountCommand(/* mapped from request */);
        var result = await dispatcher.DispatchAsync(command);
        return result.ToActionResult();
    }
}
```

Required:

1. Controllers dispatch commands via `ISystemIntentDispatcher` — no direct engine, no direct domain.
2. Reads use projection-store query handlers — never the event store directly.
3. DTOs follow `domain.guard.md` DTO naming:
   - **Request**: `{Action}{Domain}RequestModel` (e.g., `OpenAccountRequestModel`)
   - **Response**: `{Action}{Domain}ResponseModel` or `Get{Domain}ResponseModel`
   - **ReadModel** (internal): `{Domain}ReadModel`
4. Forbidden: `*Dto`, `*Response` without `Model`, `*Request` without `Model`, `*Data`, ambiguous `TodoModel` / `CardInfo`.
5. No business logic in controller — controllers are 1:1 mappers from HTTP to command.
6. Routes follow canonical `/api/{classification}/{context}/{domain}/{action}` pattern.

### Read-only controllers

Controllers can be read-only by design when the underlying aggregate is created as a side effect of another aggregate's events (e.g., `EntryController` is read-only because journal entries are created via `PostJournalEntriesCommand` on the journal aggregate). Document this asymmetry in the controller's XML doc.

## Projection layer

Reference: [src/projections/economic/](../../../src/projections/economic/)

### Projection layout

```
src/projections/{classification}/
  {context}/
    {domain}/
      {Domain}ProjectionHandler.cs
      {Domain}ProjectionReducer.cs
      {Domain}ReadModel.cs
```

### Reducer pattern

Reference: [src/projections/economic/capital/account/CapitalAccountProjectionReducer.cs](../../../src/projections/economic/capital/account/CapitalAccountProjectionReducer.cs)

Pure function — input event + previous state → new state. No I/O, no side effects, no async. Per behavioral rule 9 (NO SIDE EFFECTS IN QUERIES).

```csharp
public static class {Domain}ProjectionReducer
{
    public static {Domain}ReadModel Apply({Domain}ReadModel? state, DomainEvent evt) => evt switch
    {
        {Domain}OpenedEvent e => new {Domain}ReadModel { /* initial state */ },
        {Domain}UpdatedEvent e => state! with { /* updated fields */ },
        // exhaustive switch over all events for this domain
        _ => state!
    };
}
```

### Handler pattern

Calls the reducer + persists the read model via `IProjectionStore`:

```csharp
public sealed class {Domain}ProjectionHandler(IProjectionStore<{Domain}ReadModel> store)
    : IProjectionHandler<{Domain}ReadModel>
{
    public async Task HandleAsync(DomainEvent evt, CancellationToken ct)
    {
        var current = await store.LoadAsync(/* aggregate id from event */, ct);
        var next = {Domain}ProjectionReducer.Apply(current, evt);
        await store.SaveAsync(next, ct);
    }
}
```

### Projection quality

- Cover **every** event from every BC in the vertical.
- Replay-safe: re-applying the same events from scratch yields the same final state.
- Projections register in `{Vertical}ProjectionModule.cs` under [src/platform/host/composition/](../../../src/platform/host/composition/).

## Test layer (three tiers)

### Unit tests

Reference: [tests/unit/economic-system/](../../../tests/unit/economic-system/) (~20 files for economic vertical)

- Domain logic only (aggregates, value objects, specifications).
- No infrastructure mocks.
- No DI container.
- One file per aggregate / spec / VO.

### Integration tests

Reference: [tests/integration/economic-system/](../../../tests/integration/economic-system/) (~34 files)

- Command → middleware → engine → event → projection (in-memory).
- Real runtime pipeline, mocked external systems (Kafka, Postgres) where unavoidable.
- Replay regression tests per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01` for any aggregate with wrapper-struct VOs in events.
- Canonical replay regression template: [tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs](../../../tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs)

### E2E tests

Reference: [tests/e2e/economic/](../../../tests/e2e/economic/) (~53 files)

- Full HTTP API → controller → dispatch → engine → event → projection → API read.
- Real Postgres / Kafka via test containers or compose.
- One folder per BC: `tests/e2e/economic/{context}/{domain}/`.
- Setup pattern: `_setup/` folder with fixtures per BC.

## Quality bar (economic exemplar)

| Tier | File count | Coverage |
|---|---|---|
| Unit | 20 | Domain logic for all 12 BCs |
| Integration | 34 | Command→event for all 12 BCs |
| E2E | 53 | Full flow for all 12 BCs |
| **Total** | **107** | All 12 BCs |

A new vertical at D2 should reach a comparable count proportional to its BC count.

## Anti-drift checks before merge

- [ ] Every controller endpoint dispatches a real command (no `Ok()` placeholders).
- [ ] Every command in the vertical has at least one e2e test.
- [ ] Every event in the vertical reaches at least one projection handler.
- [ ] DTO naming complies with DTO-R1..R4 (no `*Dto`, no ambiguous names).
- [ ] No domain duplication in name when folder already implies it (e.g., `CreateCardRequestModel`, not `CreateKanbanCardRequestModel`).
