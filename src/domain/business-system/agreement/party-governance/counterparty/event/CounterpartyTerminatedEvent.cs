using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed record CounterpartyTerminatedEvent(
    [property: JsonPropertyName("AggregateId")] CounterpartyId CounterpartyId) : DomainEvent;
