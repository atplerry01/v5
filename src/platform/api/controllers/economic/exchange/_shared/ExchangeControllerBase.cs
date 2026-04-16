using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Exchange.Shared;

// Shared helper base for the two exchange domain controllers (fx + rate).
// Mirrors CapitalControllerBase in intent:
//   - Dispatch commands via ISystemIntentDispatcher with a consistent ack/failure shape.
//   - Load a JSON-state read-model row by aggregate_id from the projections DB.
//   - Query JSON-state read-model rows by indexed JSONB fields (base/quote currency pair).
public abstract class ExchangeControllerBase : ControllerBase
{
    // P-JSONB-KEY-CASE-01 remediation — deserialize the JSONB `state` column
    // using case-insensitive matching so camelCase keys (written by
    // PostgresProjectionStore) map cleanly into the PascalCase C# record
    // properties (FxReadModel.BaseCurrency, etc.). Single, reused options
    // instance per controller class (thread-safe by design of JsonSerializerOptions).
    private static readonly JsonSerializerOptions ReadModelJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    protected ISystemIntentDispatcher Dispatcher { get; }
    protected IIdGenerator IdGenerator { get; }
    protected IClock Clock { get; }
    protected string ProjectionsConnectionString { get; }

    protected ExchangeControllerBase(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        Dispatcher = dispatcher;
        IdGenerator = idGenerator;
        Clock = clock;
        ProjectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    protected async Task<IActionResult> Dispatch(
        object command,
        DomainRoute route,
        string ack,
        string failureCode,
        CancellationToken ct)
    {
        var result = await Dispatcher.DispatchAsync(command, route, ct);
        // R-CHAIN-CORRELATION-SURFACE-01 remediation: propagate the dispatcher's
        // CorrelationId into the API envelope so the value the caller sees in
        // `meta.correlationId` matches the `events.correlation_id`,
        // `whyce_chain.correlation_id`, and the Kafka `correlation-id` header.
        // Never synthesised at the controller level — always sourced from the
        // CommandResult that flowed back from RuntimeControlPlane.
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, Clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", Clock.UtcNow, result.CorrelationId));
    }

    protected async Task<IActionResult> LoadReadModel<TReadModel>(
        Guid id,
        string schema,
        string table,
        string notFoundCode,
        CancellationToken ct)
        where TReadModel : class
    {
        await using var conn = new NpgsqlConnection(ProjectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail(notFoundCode, $"{typeof(TReadModel).Name} {id} not found.", Clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TReadModel>(stateJson, ReadModelJsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize {typeof(TReadModel).Name} for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), Clock.UtcNow));
    }

    protected async Task<IActionResult> LoadReadModelsByPair<TReadModel>(
        string baseCurrency,
        string quoteCurrency,
        string schema,
        string table,
        CancellationToken ct,
        string? orderBy = null)
        where TReadModel : class
    {
        await using var conn = new NpgsqlConnection(ProjectionsConnectionString);
        await conn.OpenAsync(ct);

        var orderClause = orderBy is null ? string.Empty : $" ORDER BY {orderBy}";
        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} " +
            "WHERE state->>'baseCurrency' = @base AND state->>'quoteCurrency' = @quote" +
            orderClause, conn);
        cmd.Parameters.AddWithValue("base", baseCurrency);
        cmd.Parameters.AddWithValue("quote", quoteCurrency);

        var models = new List<TReadModel>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var stateJson = reader.GetString(0);
            var model = JsonSerializer.Deserialize<TReadModel>(stateJson, ReadModelJsonOptions)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize {typeof(TReadModel).Name} row.");
            models.Add(model);
        }

        return Ok(ApiResponse.Ok(models, this.RequestCorrelationId(), Clock.UtcNow));
    }
}
