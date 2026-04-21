using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public sealed record ApprovalCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ApprovalId ApprovalId) : DomainEvent;
