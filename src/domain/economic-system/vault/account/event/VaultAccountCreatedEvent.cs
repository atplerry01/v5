using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public sealed record VaultAccountCreatedEvent(
    VaultAccountId VaultAccountId,
    SubjectId OwnerSubjectId,
    Currency Currency) : DomainEvent;
