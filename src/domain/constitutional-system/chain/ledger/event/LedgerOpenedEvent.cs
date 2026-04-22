using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed record LedgerOpenedEvent(
    [property: JsonPropertyName("AggregateId")] LedgerId LedgerId,
    LedgerDescriptor Descriptor,
    DateTimeOffset OpenedAt) : DomainEvent;
