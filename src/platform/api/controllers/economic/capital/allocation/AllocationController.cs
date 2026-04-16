using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Allocation;

[Authorize]
[ApiController]
[Route("api/capital/allocation")]
[ApiExplorerSettings(GroupName = "economic.capital.allocation")]
public sealed class AllocationController : CapitalControllerBase
{
    private static readonly DomainRoute AllocationRoute = new("economic", "capital", "allocation");

    public AllocationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreateAllocation([FromBody] ApiRequest<CreateAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var allocationId = IdGenerator.Generate($"economic:capital:allocation:{p.SourceAccountId}:{p.TargetId}:{p.Amount}");
        var cmd = new CreateCapitalAllocationCommand(allocationId, p.SourceAccountId, p.TargetId, p.Amount, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_created", "economic.capital.allocation.create_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> ReleaseAllocation([FromBody] ApiRequest<ReleaseAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalAllocationCommand(p.AllocationId, Clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_released", "economic.capital.allocation.release_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> CompleteAllocation([FromBody] ApiRequest<CompleteAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteCapitalAllocationCommand(p.AllocationId, Clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_completed", "economic.capital.allocation.complete_failed", ct);
    }

    [HttpPost("spv")]
    public Task<IActionResult> AllocateToSpv([FromBody] ApiRequest<AllocateToSpvRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateCapitalToSpvCommand(p.AllocationId, p.SpvTargetId, p.OwnershipPercentage);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_spv_declared", "economic.capital.allocation.spv_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAllocation(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalAllocationReadModel>(
            id,
            "projection_economic_capital_allocation",
            "capital_allocation_read_model",
            "economic.capital.allocation.not_found",
            ct);
}

public sealed record CreateAllocationRequestModel(Guid SourceAccountId, Guid TargetId, decimal Amount, string Currency);
public sealed record ReleaseAllocationRequestModel(Guid AllocationId);
public sealed record CompleteAllocationRequestModel(Guid AllocationId);
public sealed record AllocateToSpvRequestModel(Guid AllocationId, string SpvTargetId, decimal OwnershipPercentage);
