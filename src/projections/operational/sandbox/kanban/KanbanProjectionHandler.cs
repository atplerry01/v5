using System.Diagnostics.Metrics;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Kanban;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.Operational.Sandbox.Kanban;

/// <summary>
/// Materializes the Kanban board read model in Postgres by merging events into a
/// per-aggregate state row (JSONB). Owns the write to
/// projection_operational_sandbox_kanban.kanban_read_model — the generic projection
/// writer is suppressed for handled events so this handler is the single source
/// of truth for the row's state.
///
/// Implements the shared envelope-based projection handler contract so this file
/// does not depend on src/runtime/**. Closes dependency-graph guard DG-R7-01.
/// </summary>
public sealed class KanbanProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<KanbanBoardCreatedEventSchema>,
    IProjectionHandler<KanbanListCreatedEventSchema>,
    IProjectionHandler<KanbanCardCreatedEventSchema>,
    IProjectionHandler<KanbanCardMovedEventSchema>,
    IProjectionHandler<KanbanCardReorderedEventSchema>,
    IProjectionHandler<KanbanCardCompletedEventSchema>,
    IProjectionHandler<KanbanCardUpdatedEventSchema>
{
    private const string Schema = "projection_operational_sandbox_kanban";
    private const string Table = "kanban_read_model";
    private const string AggregateType = "Kanban";
    private const string PoolName = "projections";

    private static readonly Meter PoolMeter = new("Whyce.Postgres", "1.0");
    private static readonly Counter<long> PoolAcquisitions =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisitions");
    private static readonly Counter<long> PoolAcquisitionFailures =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisition_failures");

    private readonly NpgsqlDataSource _dataSource;

    private Guid _currentCorrelationId = Guid.Empty;
    private Guid _currentEventId = Guid.Empty;

    public KanbanProjectionHandler(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    private async Task<NpgsqlConnection> OpenInstrumentedAsync()
    {
        try
        {
            var conn = await _dataSource.OpenConnectionAsync();
            PoolAcquisitions.Add(1, new KeyValuePair<string, object?>("pool", PoolName));
            return conn;
        }
        catch (Exception ex)
        {
            PoolAcquisitionFailures.Add(1,
                new KeyValuePair<string, object?>("pool", PoolName),
                new KeyValuePair<string, object?>("reason", ex.GetType().Name));
            throw;
        }
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _currentCorrelationId = envelope.CorrelationId;
        _currentEventId = envelope.EventId;
        return envelope.Payload switch
        {
            KanbanBoardCreatedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanListCreatedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanCardCreatedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanCardMovedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanCardReorderedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanCardCompletedEventSchema e => HandleAsync(e, cancellationToken),
            KanbanCardUpdatedEventSchema e => HandleAsync(e, cancellationToken),
            _ => throw new InvalidOperationException(
                $"KanbanProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public async Task HandleAsync(KanbanBoardCreatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };
        state = state with { Name = e.Name };
        await UpsertAsync(e.AggregateId, state, "KanbanBoardCreatedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanListCreatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };

        var lists = new List<KanbanListReadModel>(state.Lists);
        if (!lists.Exists(l => l.ListId == e.ListId))
        {
            lists.Add(new KanbanListReadModel
            {
                ListId = e.ListId,
                Name = e.Name,
                Position = e.Position
            });
            lists.Sort((a, b) => a.Position.CompareTo(b.Position));
        }

        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanListCreatedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanCardCreatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };

        var lists = new List<KanbanListReadModel>(state.Lists);
        var listIndex = lists.FindIndex(l => l.ListId == e.ListId);
        if (listIndex < 0) return;

        var list = lists[listIndex];
        var cards = new List<KanbanCardReadModel>(list.Cards);
        if (!cards.Exists(c => c.CardId == e.CardId))
        {
            cards.Add(new KanbanCardReadModel
            {
                CardId = e.CardId,
                Title = e.Title,
                Description = e.Description,
                Position = e.Position,
                IsCompleted = false
            });
            cards.Sort((a, b) => a.Position.CompareTo(b.Position));
        }

        lists[listIndex] = list with { Cards = cards };
        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanCardCreatedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanCardMovedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken);
        if (state is null) return;

        var lists = new List<KanbanListReadModel>(state.Lists);

        var fromIndex = lists.FindIndex(l => l.ListId == e.FromListId);
        var toIndex = lists.FindIndex(l => l.ListId == e.ToListId);
        if (fromIndex < 0 || toIndex < 0) return;

        var fromCards = new List<KanbanCardReadModel>(lists[fromIndex].Cards);
        var cardIndex = fromCards.FindIndex(c => c.CardId == e.CardId);
        if (cardIndex < 0) return;

        var card = fromCards[cardIndex] with { Position = e.NewPosition };
        fromCards.RemoveAt(cardIndex);
        lists[fromIndex] = lists[fromIndex] with { Cards = fromCards };

        var toCards = new List<KanbanCardReadModel>(lists[toIndex].Cards);
        toCards.Add(card);
        toCards.Sort((a, b) => a.Position.CompareTo(b.Position));
        lists[toIndex] = lists[toIndex] with { Cards = toCards };

        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanCardMovedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanCardReorderedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken);
        if (state is null) return;

        var lists = new List<KanbanListReadModel>(state.Lists);
        var listIndex = lists.FindIndex(l => l.ListId == e.ListId);
        if (listIndex < 0) return;

        var cards = new List<KanbanCardReadModel>(lists[listIndex].Cards);
        var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
        if (cardIndex < 0) return;

        cards[cardIndex] = cards[cardIndex] with { Position = e.NewPosition };
        cards.Sort((a, b) => a.Position.CompareTo(b.Position));
        lists[listIndex] = lists[listIndex] with { Cards = cards };

        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanCardReorderedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanCardCompletedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken);
        if (state is null) return;

        var lists = new List<KanbanListReadModel>(state.Lists);
        for (var i = 0; i < lists.Count; i++)
        {
            var cards = new List<KanbanCardReadModel>(lists[i].Cards);
            var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
            if (cardIndex >= 0)
            {
                cards[cardIndex] = cards[cardIndex] with { IsCompleted = true };
                lists[i] = lists[i] with { Cards = cards };
                break;
            }
        }

        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanCardCompletedEvent", cancellationToken);
    }

    public async Task HandleAsync(KanbanCardUpdatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken);
        if (state is null) return;

        var lists = new List<KanbanListReadModel>(state.Lists);
        for (var i = 0; i < lists.Count; i++)
        {
            var cards = new List<KanbanCardReadModel>(lists[i].Cards);
            var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
            if (cardIndex >= 0)
            {
                cards[cardIndex] = cards[cardIndex] with { Title = e.Title, Description = e.Description };
                lists[i] = lists[i] with { Cards = cards };
                break;
            }
        }

        state = state with { Lists = lists };
        await UpsertAsync(e.AggregateId, state, "KanbanCardUpdatedEvent", cancellationToken);
    }

    private async Task<KanbanBoardReadModel?> LoadAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        await using var conn = await OpenInstrumentedAsync();

        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {Schema}.{Table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var json = reader.GetString(0);
        return JsonSerializer.Deserialize<KanbanBoardReadModel>(json);
    }

    private async Task UpsertAsync(Guid aggregateId, KanbanBoardReadModel state, string lastEventType, CancellationToken cancellationToken)
    {
        var stateJson = JsonSerializer.Serialize(state);

        await using var conn = await OpenInstrumentedAsync();

        var sql = $"""
            INSERT INTO {Schema}.{Table}
                (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, projected_at, created_at)
            VALUES
                (@aggId, @aggType, 1, @state::jsonb, @lastEventId, @eventType, @corrId, NOW(), NOW())
            ON CONFLICT (aggregate_id) DO UPDATE SET
                current_version = {Schema}.{Table}.current_version + 1,
                state = @state::jsonb,
                last_event_id = @lastEventId,
                last_event_type = @eventType,
                projected_at = NOW()
            WHERE {Schema}.{Table}.last_event_id IS DISTINCT FROM @lastEventId
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggId", aggregateId);
        cmd.Parameters.AddWithValue("aggType", AggregateType);
        cmd.Parameters.AddWithValue("state", stateJson);
        cmd.Parameters.AddWithValue("lastEventId", _currentEventId);
        cmd.Parameters.AddWithValue("eventType", lastEventType);
        cmd.Parameters.AddWithValue("corrId", _currentCorrelationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
