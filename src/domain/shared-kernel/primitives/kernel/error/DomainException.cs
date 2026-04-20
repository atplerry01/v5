using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Canonical exception raised when a domain invariant is violated
/// (aggregate rule, value-object precondition, bounded-context
/// constraint). Implements <see cref="IDomainInvariantViolation"/> so
/// adapter layers (HTTP edge, messaging DLQ) can catch invariant
/// violations by interface without taking a reference on
/// <c>Whycespace.Domain</c> — see <c>DomainExceptionHandler</c> in
/// <c>src/platform/api/middleware/</c>, which catches by interface to
/// preserve the <c>platform_api</c> → <c>Shared</c> / <c>Systems</c>
/// layer rule (<c>dependency-check.sh</c>, DG-R5-01).
/// </summary>
public class DomainException : Exception, IDomainInvariantViolation
{
    public DomainException(string message) : base(message) { }
}
