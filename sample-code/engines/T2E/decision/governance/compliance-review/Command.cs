namespace Whycespace.Engines.T2E.Decision.Governance.ComplianceReview;

public record ComplianceReviewCommand(
    string Action,
    string EntityId,
    object Payload
);
