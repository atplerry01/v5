using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed record AccountCreatedEvent(
    AccountId AccountId,
    CustomerRef Customer,
    AccountName Name,
    AccountType Type);
