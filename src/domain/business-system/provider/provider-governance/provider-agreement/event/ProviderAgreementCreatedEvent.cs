using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAgreementId ProviderAgreementId,
    ClusterProviderRef Provider,
    ContractRef? Contract,
    TimeWindow? Effective) : DomainEvent;
