using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Revenue.Contract;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue.contract")]
public sealed class ContractController : ControllerBase
{
    private static readonly DomainRoute ContractRoute = new("economic", "revenue", "contract");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public ContractController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("contract/create")]
    public async Task<IActionResult> Create(
        [FromBody] ApiRequest<CreateContractRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var p = request.Data;
        var contractId = _idGenerator.Generate(
            $"economic:revenue:contract:{p.TermStart:O}:{p.TermEnd:O}:{p.ShareRules.Count}");

        var rules = new List<ContractShareRule>(p.ShareRules.Count);
        foreach (var r in p.ShareRules)
            rules.Add(new ContractShareRule(r.PartyId, r.SharePercentage));

        var cmd = new CreateRevenueContractCommand(contractId, rules, p.TermStart, p.TermEnd);
        var result = await _dispatcher.DispatchAsync(cmd, ContractRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("contract_created"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.contract.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("contract/{contractId:guid}/activate")]
    public async Task<IActionResult> Activate(Guid contractId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var cmd = new ActivateRevenueContractCommand(contractId);
        var result = await _dispatcher.DispatchAsync(cmd, ContractRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("contract_activated"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.contract.activate_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("contract/{contractId:guid}/terminate")]
    public async Task<IActionResult> Terminate(
        Guid contractId,
        [FromBody] ApiRequest<TerminateContractRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var cmd = new TerminateRevenueContractCommand(contractId, request.Data.Reason);
        var result = await _dispatcher.DispatchAsync(cmd, ContractRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("contract_terminated"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.contract.terminate_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record CreateContractRequestModel(
    DateTimeOffset TermStart,
    DateTimeOffset TermEnd,
    IReadOnlyList<ContractShareRuleRequestModel> ShareRules);

public sealed record ContractShareRuleRequestModel(Guid PartyId, decimal SharePercentage);

public sealed record TerminateContractRequestModel(string Reason);
