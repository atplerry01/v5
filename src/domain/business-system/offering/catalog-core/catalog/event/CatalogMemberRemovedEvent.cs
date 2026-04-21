using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed record CatalogMemberRemovedEvent(
    [property: JsonPropertyName("AggregateId")] CatalogId CatalogId,
    CatalogMember Member) : DomainEvent;
