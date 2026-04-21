using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed record AccountActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AccountId AccountId) : DomainEvent;
