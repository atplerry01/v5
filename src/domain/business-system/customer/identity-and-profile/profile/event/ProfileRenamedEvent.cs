using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed record ProfileRenamedEvent(
    [property: JsonPropertyName("AggregateId")] ProfileId ProfileId,
    ProfileDisplayName DisplayName) : DomainEvent;
