using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed record AccountCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AccountId AccountId,
    CustomerRef Customer,
    AccountName Name,
    AccountType Type) : DomainEvent;
