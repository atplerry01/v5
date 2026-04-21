using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public enum BusinessOwnerKind
{
    None,
    Agreement,
    Order,
    Customer,
    Offering,
    Provider,
    Service,
    EnforcementRule,
    Violation,
    Settlement,
    Expense,
    Kanban,
    Todo,
    Incident,
    Audit,
    Counterparty
}

/// Optional business-aggregate back-reference for content. Polymorphic by
/// kind — a content aggregate may be associated with a specific business
/// aggregate (e.g. an agreement, an order) in addition to its mandatory
/// structural owner. Carries only the kind + identifier.
public readonly record struct BusinessOwnerRef
{
    public BusinessOwnerKind Kind { get; }
    public Guid Value { get; }

    public BusinessOwnerRef(BusinessOwnerKind kind, Guid value)
    {
        Guard.Against(!Enum.IsDefined(kind), "BusinessOwnerKind is invalid.");

        if (kind == BusinessOwnerKind.None)
        {
            Guard.Against(value != Guid.Empty,
                "BusinessOwnerRef of kind 'None' must carry Guid.Empty.");
        }
        else
        {
            Guard.Against(value == Guid.Empty,
                "BusinessOwnerRef value must not be empty for a typed kind.");
        }

        Kind = kind;
        Value = value;
    }

    public static BusinessOwnerRef None => new(BusinessOwnerKind.None, Guid.Empty);

    public bool IsSet => Kind != BusinessOwnerKind.None;
}
