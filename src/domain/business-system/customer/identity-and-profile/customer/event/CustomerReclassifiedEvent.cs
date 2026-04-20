namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed record CustomerReclassifiedEvent(CustomerId CustomerId, CustomerType Type);
