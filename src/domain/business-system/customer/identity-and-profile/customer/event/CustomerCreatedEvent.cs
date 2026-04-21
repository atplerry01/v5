using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed record CustomerCreatedEvent(
    [property: JsonPropertyName("AggregateId")] CustomerId CustomerId,
    CustomerName Name,
    CustomerType Type,
    CustomerReferenceCode? ReferenceCode) : DomainEvent;
