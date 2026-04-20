namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed record CustomerCreatedEvent(
    CustomerId CustomerId,
    CustomerName Name,
    CustomerType Type,
    CustomerReferenceCode? ReferenceCode);
