using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Acceptance;

[Authorize]
[ApiController]
[Route("api/commitment/acceptance")]
[ApiExplorerSettings(GroupName = "business.agreement.commitment.acceptance")]
public sealed class AcceptanceController : BusinessControllerBase
{
    private static readonly DomainRoute AcceptanceRoute = new("business", "agreement", "acceptance");

    public AcceptanceController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAcceptanceRequestModel> request, CancellationToken ct)
    {
        var cmd = new CreateAcceptanceCommand(request.Data.AcceptanceId);
        return Dispatch(cmd, AcceptanceRoute, "acceptance_created", "business.agreement.acceptance.create_failed", ct);
    }

    [HttpPost("accept")]
    public Task<IActionResult> Accept([FromBody] ApiRequest<AcceptanceIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new AcceptAcceptanceCommand(request.Data.AcceptanceId);
        return Dispatch(cmd, AcceptanceRoute, "acceptance_accepted", "business.agreement.acceptance.accept_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<AcceptanceIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RejectAcceptanceCommand(request.Data.AcceptanceId);
        return Dispatch(cmd, AcceptanceRoute, "acceptance_rejected", "business.agreement.acceptance.reject_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAcceptance(Guid id, CancellationToken ct) =>
        LoadReadModel<AcceptanceReadModel>(
            id,
            "projection_business_agreement_acceptance",
            "acceptance_read_model",
            "business.agreement.acceptance.not_found",
            ct);
}

public sealed record CreateAcceptanceRequestModel(Guid AcceptanceId);
public sealed record AcceptanceIdRequestModel(Guid AcceptanceId);
