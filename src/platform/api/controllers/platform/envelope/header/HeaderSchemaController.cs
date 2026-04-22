using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Envelope.Header;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Envelope.Header;

[Authorize]
[ApiController]
[Route("api/platform/envelope/header")]
[ApiExplorerSettings(GroupName = "platform.envelope.header")]
public sealed class HeaderSchemaController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "envelope", "header");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public HeaderSchemaController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterHeaderSchemaRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:envelope:header:{p.HeaderKind}:{p.SchemaVersion}");
        var cmd = new RegisterHeaderSchemaCommand(id, p.HeaderKind, p.SchemaVersion, p.RequiredFields, _clock.UtcNow);
        return Dispatch(cmd, "header_schema_registered", "platform.envelope.header.register_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateHeaderSchemaRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateHeaderSchemaCommand(request.Data.HeaderSchemaId, _clock.UtcNow);
        return Dispatch(cmd, "header_schema_deprecated", "platform.envelope.header.deprecate_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterHeaderSchemaRequestModel(
    string HeaderKind,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields);

public sealed record DeprecateHeaderSchemaRequestModel(Guid HeaderSchemaId);
