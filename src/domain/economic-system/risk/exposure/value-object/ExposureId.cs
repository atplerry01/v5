using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public readonly record struct ExposureId
{
    public Guid Value { get; }

    public ExposureId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ExposureId cannot be empty.");
        Value = value;
    }
}
