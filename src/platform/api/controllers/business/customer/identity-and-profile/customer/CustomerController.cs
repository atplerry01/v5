using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.IdentityAndProfile.Customer;

[Authorize]
[ApiController]
[Route("api/identity-and-profile/customer")]
[ApiExplorerSettings(GroupName = "business.customer.identity-and-profile.customer")]
public sealed class CustomerController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute CustomerRoute = new("business", "customer", "customer");

    public CustomerController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateCustomerRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateCustomerCommand(p.CustomerId, p.Name, p.Type, p.ReferenceCode);
        return Dispatch(cmd, CustomerRoute, "customer_created", "business.customer.customer.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameCustomerRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RenameCustomerCommand(p.CustomerId, p.Name);
        return Dispatch(cmd, CustomerRoute, "customer_renamed", "business.customer.customer.rename_failed", ct);
    }

    [HttpPost("reclassify")]
    public Task<IActionResult> Reclassify([FromBody] ApiRequest<ReclassifyCustomerRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReclassifyCustomerCommand(p.CustomerId, p.Type);
        return Dispatch(cmd, CustomerRoute, "customer_reclassified", "business.customer.customer.reclassify_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<CustomerIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateCustomerCommand(request.Data.CustomerId);
        return Dispatch(cmd, CustomerRoute, "customer_activated", "business.customer.customer.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<CustomerIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveCustomerCommand(request.Data.CustomerId);
        return Dispatch(cmd, CustomerRoute, "customer_archived", "business.customer.customer.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetCustomer(Guid id, CancellationToken ct) =>
        LoadReadModel<CustomerReadModel>(
            id,
            "projection_business_customer_customer",
            "customer_read_model",
            "business.customer.customer.not_found",
            ct);
}

public sealed record CreateCustomerRequestModel(Guid CustomerId, string Name, string Type, string? ReferenceCode);
public sealed record RenameCustomerRequestModel(Guid CustomerId, string Name);
public sealed record ReclassifyCustomerRequestModel(Guid CustomerId, string Type);
public sealed record CustomerIdRequestModel(Guid CustomerId);
