using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed record ContractTerm(Timestamp StartDate, Timestamp EndDate) : ValueObject;
