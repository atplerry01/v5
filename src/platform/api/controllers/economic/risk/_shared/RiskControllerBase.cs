using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Risk.Shared;

// Shared helper base for risk-context controllers. Mirrors CapitalControllerBase:
//   - Dispatches commands via ISystemIntentDispatcher with a consistent ack/failure shape.
//   - Loads JSON-state read-model rows from the projections DB by aggregate_id
//     with a consistent not-found shape.
public abstract class RiskControllerBase : ControllerBase
{
    protected ISystemIntentDispatcher Dispatcher { get; }
    protected IIdGenerator IdGenerator { get; }
    protected IClock Clock { get; }
    protected string ProjectionsConnectionString { get; }

    protected RiskControllerBase(
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
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, Clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", Clock.UtcNow));
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
        var model = JsonSerializer.Deserialize<TReadModel>(stateJson)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize {typeof(TReadModel).Name} for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), Clock.UtcNow));
    }
}
