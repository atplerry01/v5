using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed record PackageArchivedEvent(
    [property: JsonPropertyName("AggregateId")] PackageId PackageId) : DomainEvent;
