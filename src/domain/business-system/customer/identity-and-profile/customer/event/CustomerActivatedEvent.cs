using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed record CustomerActivatedEvent(
    [property: JsonPropertyName("AggregateId")] CustomerId CustomerId) : DomainEvent;
