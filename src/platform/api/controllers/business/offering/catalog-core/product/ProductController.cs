using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CatalogCore.Product;

[Authorize]
[ApiController]
[Route("api/catalog-core/product")]
[ApiExplorerSettings(GroupName = "business.offering.catalog-core.product")]
public sealed class ProductController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ProductRoute = new("business", "offering", "product");

    public ProductController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProductRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ProductId MUST be supplied by the caller per DET-SEED-DERIVATION-01.
        var cmd = new CreateProductCommand(p.ProductId, p.Name, p.Type, p.CatalogId);
        return Dispatch(cmd, ProductRoute, "product_created", "business.offering.product.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateProductRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateProductCommand(p.ProductId, p.Name, p.Type);
        return Dispatch(cmd, ProductRoute, "product_updated", "business.offering.product.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProductIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProductCommand(request.Data.ProductId);
        return Dispatch(cmd, ProductRoute, "product_activated", "business.offering.product.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProductIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProductCommand(request.Data.ProductId);
        return Dispatch(cmd, ProductRoute, "product_archived", "business.offering.product.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProduct(Guid id, CancellationToken ct) =>
        LoadReadModel<ProductReadModel>(
            id,
            "projection_business_offering_product",
            "product_read_model",
            "business.offering.product.not_found",
            ct);
}

public sealed record CreateProductRequestModel(Guid ProductId, string Name, string Type, Guid? CatalogId);
public sealed record UpdateProductRequestModel(Guid ProductId, string Name, string Type);
public sealed record ProductIdRequestModel(Guid ProductId);
