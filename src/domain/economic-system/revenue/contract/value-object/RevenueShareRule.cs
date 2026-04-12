using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed record RevenueShareRule(Guid PartyId, decimal SharePercentage) : ValueObject;
