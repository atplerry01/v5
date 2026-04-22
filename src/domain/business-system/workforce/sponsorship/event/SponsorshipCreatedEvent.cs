using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Sponsorship;

public sealed record SponsorshipCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SponsorshipId SponsorshipId,
    SponsorshipDescriptor Descriptor) : DomainEvent;
