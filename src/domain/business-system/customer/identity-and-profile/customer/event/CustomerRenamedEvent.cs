namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed record CustomerRenamedEvent(CustomerId CustomerId, CustomerName Name);
