using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Contract;

[Authorize]
[ApiController]
[Route("api/contract")]
[ApiExplorerSettings(GroupName = "business.agreement.commitment.contract")]
public sealed class ContractController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ContractRoute = new("business", "agreement", "contract");

    public ContractController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateContractRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ContractId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01 — no
        // clock/Random entropy on the API hot path). CreatedAt is read from
        // IClock (host-bound wall clock in production; TestClock in tests) so
        // the value flows through the event-sourced invariant and lands on the
        // ContractCreatedEvent without leaking DateTimeOffset.UtcNow into
        // controller code.
        var cmd = new CreateContractCommand(p.ContractId, Clock.UtcNow);
        return Dispatch(cmd, ContractRoute, "contract_created", "business.agreement.contract.create_failed", ct);
    }

    [HttpPost("add-party")]
    public Task<IActionResult> AddParty([FromBody] ApiRequest<AddPartyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddPartyToContractCommand(p.ContractId, p.PartyId, p.Role);
        return Dispatch(cmd, ContractRoute, "contract_party_added", "business.agreement.contract.add_party_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ContractIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateContractCommand(request.Data.ContractId);
        return Dispatch(cmd, ContractRoute, "contract_activated", "business.agreement.contract.activate_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<ContractIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SuspendContractCommand(request.Data.ContractId);
        return Dispatch(cmd, ContractRoute, "contract_suspended", "business.agreement.contract.suspend_failed", ct);
    }

    [HttpPost("terminate")]
    public Task<IActionResult> Terminate([FromBody] ApiRequest<ContractIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new TerminateContractCommand(request.Data.ContractId);
        return Dispatch(cmd, ContractRoute, "contract_terminated", "business.agreement.contract.terminate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetContract(Guid id, CancellationToken ct) =>
        LoadReadModel<ContractReadModel>(
            id,
            "projection_business_agreement_contract",
            "contract_read_model",
            "business.agreement.contract.not_found",
            ct);
}

public sealed record CreateContractRequestModel(Guid ContractId);
public sealed record AddPartyRequestModel(Guid ContractId, Guid PartyId, string Role);
public sealed record ContractIdRequestModel(Guid ContractId);
