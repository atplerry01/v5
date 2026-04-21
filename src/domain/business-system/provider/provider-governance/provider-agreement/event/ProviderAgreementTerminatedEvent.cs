using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementTerminatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAgreementId ProviderAgreementId,
    DateTimeOffset TerminatedAt) : DomainEvent;
