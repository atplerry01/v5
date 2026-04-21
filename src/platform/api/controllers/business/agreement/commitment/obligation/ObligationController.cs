using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Obligation;

[Authorize]
[ApiController]
[Route("api/commitment/obligation")]
[ApiExplorerSettings(GroupName = "business.agreement.commitment.obligation")]
public sealed class ObligationController : BusinessControllerBase
{
    private static readonly DomainRoute ObligationRoute = new("business", "agreement", "obligation");

    public ObligationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateObligationRequestModel> request, CancellationToken ct)
    {
        var cmd = new CreateObligationCommand(request.Data.ObligationId);
        return Dispatch(cmd, ObligationRoute, "obligation_created", "business.agreement.obligation.create_failed", ct);
    }

    [HttpPost("fulfill")]
    public Task<IActionResult> Fulfill([FromBody] ApiRequest<ObligationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new FulfillObligationCommand(request.Data.ObligationId);
        return Dispatch(cmd, ObligationRoute, "obligation_fulfilled", "business.agreement.obligation.fulfill_failed", ct);
    }

    [HttpPost("breach")]
    public Task<IActionResult> Breach([FromBody] ApiRequest<ObligationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new BreachObligationCommand(request.Data.ObligationId);
        return Dispatch(cmd, ObligationRoute, "obligation_breached", "business.agreement.obligation.breach_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetObligation(Guid id, CancellationToken ct) =>
        LoadReadModel<ObligationReadModel>(
            id,
            "projection_business_agreement_obligation",
            "obligation_read_model",
            "business.agreement.obligation.not_found",
            ct);
}

public sealed record CreateObligationRequestModel(Guid ObligationId);
public sealed record ObligationIdRequestModel(Guid ObligationId);
