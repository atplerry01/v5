namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public static class PortfolioErrors
{
    public static PortfolioDomainException MissingId()
        => new("PortfolioId is required and must not be empty.");

    public static PortfolioDomainException InvalidStateTransition(PortfolioStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static PortfolioDomainException NameRequired()
        => new("Portfolio must have a name.");

    public static PortfolioDomainException AlreadyTerminated()
        => new("Portfolio has been terminated and cannot be modified.");
}

public sealed class PortfolioDomainException : Exception
{
    public PortfolioDomainException(string message) : base(message) { }
}
