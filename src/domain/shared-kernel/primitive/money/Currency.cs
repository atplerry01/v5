using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        Guard.Against(string.IsNullOrWhiteSpace(code), "Currency code cannot be null or whitespace.");
        Code = code;
    }
}
