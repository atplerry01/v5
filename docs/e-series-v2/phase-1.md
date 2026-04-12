# PHASE 1 -- CORE EXECUTION SYSTEM v2 (POST-KANBAN VALIDATED)

## SCOPE: E1 - E8 + API + E2E Proof

## VERSION: 2.0 -- Incorporates all lessons from Kanban live implementation

---

# IMPLEMENTATION FLOW (STRICT ORDER)

```
Stage 0: Domain Placement
  E1: Domain Model
  E2: Contracts & Event Definitions
  E5: Engine (T2E)
  E6: Runtime Wiring + Bootstrap + OPA + Kafka + Projection DB
  E7: Event Fabric Hardening
  E8: Projection Handler
  EX: API Controller
  EX+: Live Validation
```

E3 (Persistence) and E4 (Determinism) are not separate stages -- they are
continuous constraints enforced at every stage via guards. E9-E12 (Policy,
Guards, Chain, Pipeline) are infrastructure-provided and activated automatically
through E6 wiring.

---

# STAGE 0 -- DOMAIN PLACEMENT

Define before any code:

* classification (e.g. operational)
* context (e.g. sandbox)
* domain (e.g. kanban)
* aggregate name (e.g. KanbanAggregate -- ONE only)

Create directory:

```
src/domain/{classification}-system/{context}/{domain}/
```

CRITICAL: The -system suffix is ONLY on domain classification folders.
Contracts, engines, projections, systems, and API controllers use the
classification name WITHOUT -system.

---

# E1 -- DOMAIN MODEL

## Location

```
src/domain/{classification}-system/{context}/{domain}/
  aggregate/    {Name}Aggregate.cs
  entity/       {Name}.cs
  event/        {Name}{Action}Event.cs
  value-object/ {Name}Id.cs, {Name}Status.cs, etc.
  error/        {Domain}Errors.cs (or {Domain}DomainErrors.cs)
```

## Conventions (Validated)

### Aggregate

```csharp
public sealed class KanbanAggregate : AggregateRoot
{
    // Private parameterless ctor (REQUIRED for LoadFromHistory reflection)
    private KanbanAggregate() { }

    // Static factory (ONLY way to create)
    public static KanbanAggregate Create(KanbanBoardId id, string name)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), KanbanDomainErrors.BoardNameRequired);
        var aggregate = new KanbanAggregate { Id = id };
        aggregate.RaiseDomainEvent(new KanbanBoardCreatedEvent(new AggregateId(id.Value), name));
        return aggregate;
    }

    // Command methods (raise events, never mutate directly)
    // Override Apply(object) for event sourcing
    // Override EnsureInvariants() for pre-change validation
}
```

### Value Objects

```csharp
// Simple ID -- readonly record struct
public readonly record struct KanbanBoardId(Guid Value);

// Enums for finite states
public enum KanbanStatus { Backlog, Todo, InProgress, Review, Done }

// Position/count -- readonly record struct
public readonly record struct KanbanPosition(int Value);
```

LESSON: Do NOT add explicit constructors to positional record structs.
`public readonly record struct XxxId(Guid Value) { public XxxId(Guid value) : this() { ... } }`
causes CS0111. Either use positional parameters OR explicit properties, not both.

### Events

```csharp
public sealed record KanbanBoardCreatedEvent(AggregateId AggregateId, string Name) : DomainEvent;
```

All events MUST have AggregateId as first parameter (used by outbox reflection).

### Errors

```csharp
public static class KanbanDomainErrors
{
    public const string ListNotFound = "The specified list does not exist on this board.";
}
```

## Prohibitions

* NO Guid.NewGuid(), DateTime.UtcNow, Random
* NO infrastructure references
* ONLY using Whycespace.Domain.SharedKernel.Primitives.Kernel allowed

---

# E2 -- CONTRACTS & EVENT DEFINITIONS

## Location

```
src/shared/contracts/operational/sandbox/kanban/   (commands, queries, intents, DTOs)
src/shared/contracts/events/kanban/                (event schemas)
```

## Commands -- CRITICAL CONVENTION

The command MUST have `Id` as the FIRST positional parameter, representing
the aggregate ID. The SystemIntentDispatcher extracts it via
`GetProperty("Id")` reflection.

```csharp
// CORRECT: Id is first, represents aggregate (board) ID
public sealed record CreateKanbanListCommand(Guid Id, Guid ListId, string Name, int Position);

// WRONG: BoardId instead of Id -- dispatcher cannot find aggregate ID
public sealed record CreateKanbanListCommand(Guid ListId, Guid BoardId, string Name, int Position);
```

LESSON: For multi-entity domains (Board owns Lists owns Cards), ALL commands
target the aggregate root. Id = BoardId for ALL kanban commands.

## Event Schemas -- CRITICAL CONVENTION

Schema records use PRIMITIVE types (Guid, int, string), NOT value objects.
The outbox serializes raw domain events (which use value objects). The consumer
deserializes as schema records (which expect primitives). The FlexibleGuidConverter
and FlexibleIntConverter bridge the gap.

```csharp
// CORRECT: primitives
public sealed record KanbanListCreatedEventSchema(Guid AggregateId, Guid ListId, string Name, int Position);

// WRONG: value objects -- consumer cannot deserialize
public sealed record KanbanListCreatedEventSchema(AggregateId AggregateId, KanbanListId ListId, string Name, KanbanPosition Position);
```

## Intents

Sealed records carrying user-facing input + UserId. No domain types.

## DTOs

Sealed records with init properties for API responses.

---

# E5 -- ENGINE (T2E)

## Location

```
src/engines/T2E/{classification}/{context}/{domain}/
  {Action}{Domain}Handler.cs    (one file per command)
```

## Convention

One handler per command. Implements IEngine. Pattern:

```csharp
public sealed class CreateKanbanBoardHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateKanbanBoardCommand cmd)
            return Task.CompletedTask;

        // Create: construct aggregate directly
        var aggregate = KanbanAggregate.Create(
            new KanbanBoardId(context.AggregateId), cmd.Name);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}

public sealed class CreateKanbanListHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateKanbanListCommand cmd) return;

        // Mutate: load aggregate then call domain method
        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.CreateList(new KanbanListId(cmd.ListId), cmd.Name, new KanbanPosition(cmd.Position));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
```

Create commands = synchronous (no LoadAggregate).
Mutate commands = async (LoadAggregate, which replays events via Apply).

---

# E6 -- RUNTIME WIRING (MOST COMPLEX STAGE)

This stage creates/modifies files across 6 locations. Every item is mandatory.

## 1. Bootstrap Module

```
src/platform/host/composition/{classification}/{context}/{domain}/{Domain}Bootstrap.cs
```

Implements IDomainBootstrapModule. Registers:

* T2E handlers (services.AddTransient per command handler)
* Intent handler (systems layer)
* Projection handler (services.AddSingleton with ProjectionsDataSource.Inner)
* Kafka consumer worker (services.AddSingleton<IHostedService>, NOT AddHostedService)
* Engine bindings (engine.Register<TCommand, THandler>)
* Schema bindings (DomainSchemaCatalog)
* Projection bindings (projection.Register per event type)

LESSON: AddHostedService<T>(factory) uses TryAddEnumerable which deduplicates
by implementation type. When multiple domains use GenericKafkaProjectionConsumerWorker,
the second registration is silently dropped. Use AddSingleton<IHostedService>
instead.

## 2. Catalog Registration

```
src/platform/host/composition/BootstrapModuleCatalog.cs
```

Add one line: `new KanbanBootstrap()` to the All list.

## 3. Schema Module

```
src/runtime/event-fabric/domain-schemas/{Domain}SchemaModule.cs
```

Register ALL events (schema type + payload mapper) via ISchemaSink.

```
src/runtime/event-fabric/domain-schemas/DomainSchemaCatalog.cs
```

Add one-line dispatch method.

## 4. Systems Layer (Intent Handler)

```
src/systems/downstream/{classification}/{context}/{domain}/
  I{Domain}IntentHandler.cs
  {Domain}IntentHandler.cs
  {Domain}SystemResult.cs
```

Maps intents to commands. Dispatches via ISystemIntentDispatcher.
Generates deterministic IDs via IIdGenerator.Generate(seed).

## 5. OPA Policy

```
infrastructure/policy/domain/{classification}/{context}/{domain}.rego
```

Must cover all actions the domain exposes. Action names derived from command
types: {domain}.{verb} where verb = lowercase prefix before domain name in
command type.

LESSON: OPA policies must be loaded into the running OPA server. Without a
matching policy, all commands get "OPA policy denied". Load via:
`curl -X PUT http://opa:8181/v1/policies/... --data-binary @kanban.rego`

## 6. Kafka Topics

```
infrastructure/event-fabric/kafka/topics/{classification}/{context}/{domain}/topics.json
```

Create the actual topics in Kafka:
```bash
kafka-topics.sh --create --topic whyce.{classification}.{context}.{domain}.events
kafka-topics.sh --create --topic whyce.{classification}.{context}.{domain}.deadletter
kafka-topics.sh --create --topic whyce.{classification}.{context}.{domain}.retry
```

LESSON: Topics must exist BEFORE the consumer starts. If missing, the consumer
throws ConsumeException and stops processing. The outbox publisher retries
with exponential backoff but eventually marks entries as deadlettered.

## 7. Projection DB Schema

```
infrastructure/data/postgres/projections/{classification}/{context}/{domain}/001_projection.sql
```

Must be applied to the projections database before projections can write.

---

# E7 -- EVENT FABRIC

## Payload Mapper Alignment

The KanbanSchemaModule payload mappers must unwrap value objects to primitives:

```csharp
sink.RegisterPayloadMapper("KanbanListCreatedEvent", e =>
{
    var evt = (DomainEvents.KanbanListCreatedEvent)e;
    return new KanbanListCreatedEventSchema(
        evt.AggregateId.Value,    // AggregateId -> Guid
        evt.ListId.Value,         // KanbanListId -> Guid
        evt.Name,                 // string -> string
        evt.Position.Value);      // KanbanPosition -> int
});
```

## FlexibleIntConverter (LESSON)

The EventDeserializer needs FlexibleIntConverter alongside FlexibleGuidConverter.
Domain events serialize value objects as `{"Value": N}` but schema records expect
raw `N`. Without this converter, all events with position/count fields fail to
deserialize in the consumer.

## Outbox Aggregate ID Extraction

The PostgresOutboxAdapter extracts aggregate_id from domain events via reflection:
`GetProperty("AggregateId")` -> unwraps `.Value` for value object types.
ALL domain events MUST have AggregateId as a property.

## Audit Emission OCC Isolation (LESSON)

The EventFabric must use ExpectedVersion = -1 for audit emissions
(aggregateIdOverride != null). Audit events go to a SEPARATE aggregate stream.
Using the domain aggregate's ExpectedVersion causes concurrency conflicts.

---

# E8 -- PROJECTIONS

## Location

```
src/projections/{classification}/{context}/{domain}/
  {Domain}ProjectionHandler.cs
  {Domain}ReadModel.cs
```

## Read Model Design

For hierarchical domains (Board -> Lists -> Cards), the read model is a single
JSONB document per aggregate:

```csharp
public sealed record KanbanBoardReadModel
{
    [JsonPropertyName("boardId")]     public Guid BoardId { get; init; }
    [JsonPropertyName("name")]        public string Name { get; init; } = string.Empty;
    [JsonPropertyName("lists")]       public List<KanbanListReadModel> Lists { get; init; } = [];
}
```

LESSON: Use [JsonPropertyName] with camelCase on read models. The projection
writes camelCase JSON. The API controller's JsonSerializer.Deserialize needs
PropertyNameCaseInsensitive = true to read it back into PascalCase DTOs.

## Handler Pattern

```csharp
public sealed class KanbanProjectionHandler : IEnvelopeProjectionHandler
{
    public Task HandleAsync(IEventEnvelope envelope, CancellationToken ct)
    {
        return envelope.Payload switch
        {
            KanbanBoardCreatedEventSchema e => HandleAsync(e, ct),
            // ... all events
        };
    }

    // Per-event: Load -> Apply -> Upsert
    public async Task HandleAsync(KanbanBoardCreatedEventSchema e, CancellationToken ct)
    {
        var state = await LoadAsync(e.AggregateId, ct) ?? new KanbanBoardReadModel { BoardId = e.AggregateId };
        state = state with { Name = e.Name };
        await UpsertAsync(e.AggregateId, state, "KanbanBoardCreatedEvent", ct);
    }
}
```

## Idempotency

```sql
ON CONFLICT (aggregate_id) DO UPDATE SET ...
WHERE schema.table.last_event_id IS DISTINCT FROM @lastEventId
```

---

# EX -- API CONTROLLER

## Location

```
src/platform/api/controllers/{classification}/{context}/{domain}/{Domain}Controller.cs
```

## Convention

ALWAYS use request DTOs. NEVER bind directly to command records.

```csharp
// CORRECT: Request DTO decouples API contract from command record
[HttpPost("card/move")]
public async Task<IActionResult> MoveCard([FromBody] MoveCardRequest request, CancellationToken ct)
{
    var cmd = new MoveKanbanCardCommand(request.BoardId, request.CardId, ...);
    var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, ct);
    return result.IsSuccess ? Ok(...) : BadRequest(...);
}

// WRONG: Direct command binding -- JSON deserialization fails when parameter
// names don't match or Id convention causes confusion
[HttpPost("card/move")]
public async Task<IActionResult> MoveCard([FromBody] MoveKanbanCardCommand cmd, CancellationToken ct) { ... }
```

## Deterministic ID Generation

Generate IDs at the API boundary using stable seeds:

```csharp
var boardId = _idGenerator.Generate($"kanban:board:{request.UserId}:{request.Name}");
```

Same inputs always produce the same ID (idempotent under retry).

---

# EX+ -- LIVE VALIDATION CHECKLIST

A domain is NOT complete until ALL pass against a running system:

## Pre-flight (Before First Request)

* [ ] Kafka topics created (events, deadletter, retry)
* [ ] Projection DB schema applied
* [ ] OPA policy loaded
* [ ] Idempotency table exists
* [ ] Both consumer workers running (check logs for "Kafka consumer config applied")

## Execution Proof (For Each Operation)

* [ ] HTTP 200 returned with correlationId
* [ ] EventStore: event row with correct version
* [ ] WhyceChain: block linked with previous_block_hash
* [ ] Outbox: status = published
* [ ] Projection: state updated correctly

## System Properties

* [ ] Idempotency: duplicate command returns "Duplicate command detected", no new events
* [ ] Concurrency: simultaneous writes produce proper 409 OCC conflicts, no corruption
* [ ] Load: 20+ rapid requests all succeed, system stable
* [ ] GET endpoint: returns correct hierarchical state

---

# KNOWN PITFALLS (FROM KANBAN IMPLEMENTATION)

1. **Command Id convention**: SystemIntentDispatcher uses reflection on `GetProperty("Id")`.
   All commands MUST have `Id` as a positional parameter = aggregate root ID.

2. **Hosted service dedup**: `AddHostedService<T>` deduplicates by type. Use
   `AddSingleton<IHostedService>` for multiple consumers of the same worker type.

3. **Value object serialization**: Outbox serializes raw domain events. Consumer
   deserializes as schema records. FlexibleIntConverter bridges `{"Value":N}` -> `N`.

4. **Idempotency key granularity**: Key must be `CommandType:CommandId` (unique per
   command instance), not `CommandType:AggregateId` (blocks all commands of same type
   on same aggregate).

5. **Audit emission OCC**: Audit events use separate aggregate streams.
   ExpectedVersion must be -1, not inherited from domain aggregate.

6. **OPA policy**: Must exist before any command can execute. Action format:
   `{domain}.{verb}` where verb = lowercase prefix stripped from command type name.

7. **Kafka topics**: Must exist before consumer starts. Missing topics crash the
   consumer worker.

8. **Projection JSON casing**: Read models use camelCase JsonPropertyName. API
   deserialization needs PropertyNameCaseInsensitive = true.

9. **Request DTOs vs command binding**: Never bind command records directly to
   API endpoints. Use request DTOs and construct commands explicitly.

10. **Record struct constructors**: Never add explicit constructors to positional
    record structs -- causes CS0111.
