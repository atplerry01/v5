using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.ContentPolicy;

public sealed record PolicyRevision : ValueObject
{
    public int Number { get; }
    public string Body { get; }

    private PolicyRevision(int number, string body)
    {
        Number = number;
        Body = body;
    }

    public static PolicyRevision Create(int number, string body)
    {
        if (number <= 0) throw ContentPolicyErrors.InvalidRevision();
        if (string.IsNullOrWhiteSpace(body)) throw ContentPolicyErrors.InvalidBody();
        return new PolicyRevision(number, body.Trim());
    }
}
