using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed record AccountRenamedEvent(
    [property: JsonPropertyName("AggregateId")] AccountId AccountId,
    AccountName Name) : DomainEvent;
