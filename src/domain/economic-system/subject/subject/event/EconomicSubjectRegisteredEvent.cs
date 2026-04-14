using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public sealed record EconomicSubjectRegisteredEvent(
    SubjectId SubjectId,
    SubjectType SubjectType,
    StructuralRef StructuralRef,
    EconomicRef EconomicRef) : DomainEvent;
