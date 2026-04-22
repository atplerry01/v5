using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Envelope.Payload;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Envelope.Payload;

[Authorize]
[ApiController]
[Route("api/platform/envelope/payload")]
[ApiExplorerSettings(GroupName = "platform.envelope.payload")]
public sealed class PayloadSchemaController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "envelope", "payload");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public PayloadSchemaController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterPayloadSchemaRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:envelope:payload:{p.TypeRef}:{p.Encoding}");
        var cmd = new RegisterPayloadSchemaCommand(id, p.TypeRef, p.Encoding, p.SchemaRef,
            p.SchemaContractVersion, p.MaxSizeBytes, _clock.UtcNow);
        return Dispatch(cmd, "payload_schema_registered", "platform.envelope.payload.register_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecatePayloadSchemaRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecatePayloadSchemaCommand(request.Data.PayloadSchemaId, _clock.UtcNow);
        return Dispatch(cmd, "payload_schema_deprecated", "platform.envelope.payload.deprecate_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterPayloadSchemaRequestModel(
    string TypeRef,
    string Encoding,
    string? SchemaRef,
    int SchemaContractVersion,
    long? MaxSizeBytes);

public sealed record DeprecatePayloadSchemaRequestModel(Guid PayloadSchemaId);
