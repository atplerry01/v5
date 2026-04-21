using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public sealed record SignatureSignedEvent(
    [property: JsonPropertyName("AggregateId")] SignatureId SignatureId) : DomainEvent;
