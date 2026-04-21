using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed record ProfileArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ProfileId ProfileId) : DomainEvent;
