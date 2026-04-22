using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed record LedgerSealedEvent(
    [property: JsonPropertyName("AggregateId")] LedgerId LedgerId,
    DateTimeOffset SealedAt) : DomainEvent;
