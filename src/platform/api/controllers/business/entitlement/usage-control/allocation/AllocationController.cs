using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.Allocation;

[Authorize]
[ApiController]
[Route("api/usage-control/allocation")]
[ApiExplorerSettings(GroupName = "business.entitlement.usage-control.allocation")]
public sealed class AllocationController : BusinessControllerBase
{
    private static readonly DomainRoute AllocationRoute = new("business", "entitlement", "allocation");

    public AllocationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateAllocationCommand(p.AllocationId, p.ResourceId, p.RequestedCapacity);
        return Dispatch(cmd, AllocationRoute, "allocation_created", "business.entitlement.allocation.create_failed", ct);
    }

    [HttpPost("allocate")]
    public Task<IActionResult> Allocate([FromBody] ApiRequest<AllocationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new AllocateAllocationCommand(request.Data.AllocationId);
        return Dispatch(cmd, AllocationRoute, "allocation_allocated", "business.entitlement.allocation.allocate_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> Release([FromBody] ApiRequest<AllocationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ReleaseAllocationCommand(request.Data.AllocationId);
        return Dispatch(cmd, AllocationRoute, "allocation_released", "business.entitlement.allocation.release_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAllocation(Guid id, CancellationToken ct) =>
        LoadReadModel<AllocationReadModel>(
            id,
            "projection_business_entitlement_allocation",
            "allocation_read_model",
            "business.entitlement.allocation.not_found",
            ct);
}

public sealed record CreateAllocationRequestModel(Guid AllocationId, Guid ResourceId, int RequestedCapacity);
public sealed record AllocationIdRequestModel(Guid AllocationId);
