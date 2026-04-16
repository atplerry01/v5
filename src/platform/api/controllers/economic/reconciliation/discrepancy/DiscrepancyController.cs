using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Reconciliation.Discrepancy;

[Authorize]
[ApiController]
[Route("api/economic/reconciliation/discrepancy")]
[ApiExplorerSettings(GroupName = "economic.reconciliation.discrepancy")]
public sealed class DiscrepancyController : ControllerBase
{
    private static readonly DomainRoute DiscrepancyRoute = new("economic", "reconciliation", "discrepancy");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public DiscrepancyController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect(
        [FromBody] ApiRequest<DetectDiscrepancyRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var discrepancyId = _idGenerator.Generate(
            $"economic:reconciliation:discrepancy:{p.ProcessReference}:{p.Source}:{p.ExpectedValue}:{p.ActualValue}");

        var command = new DetectDiscrepancyCommand(
            discrepancyId,
            p.ProcessReference,
            p.Source,
            p.ExpectedValue,
            p.ActualValue,
            p.Difference,
            _clock.UtcNow);

        var result = await _dispatcher.DispatchAsync(command, DiscrepancyRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("discrepancy_detected"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.discrepancy.detect_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("investigate")]
    public async Task<IActionResult> Investigate(
        [FromBody] ApiRequest<DiscrepancyIdRequestModel> request,
        CancellationToken cancellationToken)
    {
        var command = new InvestigateDiscrepancyCommand(request.Data.DiscrepancyId);
        var result = await _dispatcher.DispatchAsync(command, DiscrepancyRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("discrepancy_investigating"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.discrepancy.investigate_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve(
        [FromBody] ApiRequest<ResolveDiscrepancyRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new ResolveDiscrepancyCommand(p.DiscrepancyId, p.Resolution);
        var result = await _dispatcher.DispatchAsync(command, DiscrepancyRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("discrepancy_resolved"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.discrepancy.resolve_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record DetectDiscrepancyRequestModel(
    Guid ProcessReference,
    string Source,
    decimal ExpectedValue,
    decimal ActualValue,
    decimal Difference);

public sealed record DiscrepancyIdRequestModel(Guid DiscrepancyId);

public sealed record ResolveDiscrepancyRequestModel(
    Guid DiscrepancyId,
    string Resolution);
