using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed record PackageMemberRemovedEvent(
    [property: JsonPropertyName("AggregateId")] PackageId PackageId,
    PackageMember Member) : DomainEvent;
