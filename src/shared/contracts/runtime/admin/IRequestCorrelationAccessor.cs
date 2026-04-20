namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B — accessor for the per-request correlation id established by the
/// platform correlation middleware. The host implementation reads from the
/// ambient HTTP context; test doubles return a fixed value. Decoupled from
/// <c>Microsoft.AspNetCore.*</c> so the runtime layer can depend on it.
/// </summary>
public interface IRequestCorrelationAccessor
{
    Guid Current { get; }
}
