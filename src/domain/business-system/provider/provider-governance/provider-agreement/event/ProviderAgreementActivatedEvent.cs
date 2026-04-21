using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAgreementId ProviderAgreementId,
    TimeWindow Effective) : DomainEvent;
