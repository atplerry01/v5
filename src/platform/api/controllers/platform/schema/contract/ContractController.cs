using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Schema.Contract;

[Authorize]
[ApiController]
[Route("api/platform/schema/contract")]
[ApiExplorerSettings(GroupName = "platform.schema.contract")]
public sealed class ContractController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "schema", "contract");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public ContractController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterContractRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:schema:contract:{p.ContractName}:{p.PublisherClassification}:{p.PublisherDomain}");
        var cmd = new RegisterContractCommand(id, p.ContractName,
            p.PublisherClassification, p.PublisherContext, p.PublisherDomain,
            p.SchemaRef, p.SchemaVersion, _clock.UtcNow);
        return Dispatch(cmd, "contract_registered", "platform.schema.contract.register_failed", ct);
    }

    [HttpPost("add-subscriber")]
    public Task<IActionResult> AddSubscriber([FromBody] ApiRequest<AddContractSubscriberRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddContractSubscriberCommand(p.ContractId,
            p.SubscriberClassification, p.SubscriberContext, p.SubscriberDomain,
            p.MinSchemaVersion, p.RequiredCompatibilityMode, _clock.UtcNow);
        return Dispatch(cmd, "contract_subscriber_added", "platform.schema.contract.add_subscriber_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateContractRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateContractCommand(request.Data.ContractId, _clock.UtcNow);
        return Dispatch(cmd, "contract_deprecated", "platform.schema.contract.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_schema_contract.contract_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.schema.contract.not_found", $"Contract {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<ContractReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize ContractReadModel for {id}.");
        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }

    private Guid RequestCorrelationId()
    {
        if (HttpContext is { } ctx
            && ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            && Guid.TryParse(values.ToString(), out var parsed))
            return parsed;
        return Guid.Empty;
    }
}

public sealed record RegisterContractRequestModel(
    string ContractName,
    string PublisherClassification,
    string PublisherContext,
    string PublisherDomain,
    Guid SchemaRef,
    int SchemaVersion);

public sealed record AddContractSubscriberRequestModel(
    Guid ContractId,
    string SubscriberClassification,
    string SubscriberContext,
    string SubscriberDomain,
    int MinSchemaVersion,
    string RequiredCompatibilityMode);

public sealed record DeprecateContractRequestModel(Guid ContractId);
