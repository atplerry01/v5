using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.PartyGovernance.Counterparty;

[Authorize]
[ApiController]
[Route("api/party-governance/counterparty")]
[ApiExplorerSettings(GroupName = "business.agreement.party-governance.counterparty")]
public sealed class CounterpartyController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute CounterpartyRoute = new("business", "agreement", "counterparty");

    public CounterpartyController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateCounterpartyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // CounterpartyId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01 — no
        // clock/Random entropy on the API hot path).
        var cmd = new CreateCounterpartyCommand(p.CounterpartyId, p.IdentityReference, p.Name);
        return Dispatch(cmd, CounterpartyRoute, "counterparty_created", "business.agreement.counterparty.create_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<CounterpartyIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SuspendCounterpartyCommand(request.Data.CounterpartyId);
        return Dispatch(cmd, CounterpartyRoute, "counterparty_suspended", "business.agreement.counterparty.suspend_failed", ct);
    }

    [HttpPost("terminate")]
    public Task<IActionResult> Terminate([FromBody] ApiRequest<CounterpartyIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new TerminateCounterpartyCommand(request.Data.CounterpartyId);
        return Dispatch(cmd, CounterpartyRoute, "counterparty_terminated", "business.agreement.counterparty.terminate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetCounterparty(Guid id, CancellationToken ct) =>
        LoadReadModel<CounterpartyReadModel>(
            id,
            "projection_business_agreement_counterparty",
            "counterparty_read_model",
            "business.agreement.counterparty.not_found",
            ct);
}

public sealed record CreateCounterpartyRequestModel(Guid CounterpartyId, Guid IdentityReference, string Name);
public sealed record CounterpartyIdRequestModel(Guid CounterpartyId);
