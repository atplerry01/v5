package whyce.policy.business.agreement.counterparty

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.party-governance.counterparty.create
# Authoring party (owner) OR Operator (system-provisioning) may create.
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.counterparty.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.counterparty.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.party-governance.counterparty.suspend
# Admin only (operational safety action).
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.counterparty.suspend"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.party-governance.counterparty.terminate
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.counterparty.terminate"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every counterparty allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/agreement/party-governance/counterparty still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "counterparty"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
