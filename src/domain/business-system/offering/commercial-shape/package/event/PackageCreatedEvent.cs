using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed record PackageCreatedEvent(
    [property: JsonPropertyName("AggregateId")] PackageId PackageId,
    PackageCode Code,
    PackageName Name) : DomainEvent;
