namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct ExchangeRate(decimal Rate, Currency From, Currency To);
