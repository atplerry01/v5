using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed record ProfileCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProfileId ProfileId,
    CustomerRef Customer,
    ProfileDisplayName DisplayName) : DomainEvent;
