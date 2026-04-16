using System.Text.Json.Serialization;
using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

// D10: the [property: JsonPropertyName("AggregateId")] mapping bridges the
// schema-mapped JSONB on disk (which serializes the value-object id under the
// generic key "AggregateId") to the domain event's typed parameter on replay.
// Without this mapping the constructor parameter is missing on STJ deserialize
// and VaultAccountId falls through as default(VaultAccountId), which in turn
// makes the aggregate's Apply() set Currency from a partially-defaulted event
// — the symptom that surfaced as "Currency mismatch: requires '' but received 'USD'".
public sealed record VaultAccountCreatedEvent(
    [property: JsonPropertyName("AggregateId")] VaultAccountId VaultAccountId,
    SubjectId OwnerSubjectId,
    Currency Currency) : DomainEvent;
