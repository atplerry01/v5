package whyce.policy.business.agreement.contract

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.commitment.contract.create
# Authoring party (owner) OR Operator (system-provisioning) may create.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.contract.add_party
# Only the owner of the contract or an operator may add parties.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.add_party"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.add_party"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.contract.activate
# Owner or operator; activation also needs at least one party — the engine
# enforces that invariant too, this rule just gates on subject/role.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.activate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.activate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.commitment.contract.suspend
# Admin only (operational safety action).
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.suspend"
    input.subject.role == "admin"
    valid_resource
}

# whyce.business.agreement.commitment.contract.terminate
# Admin only — terminal state.
allow if {
    input.policy_id == "whyce.business.agreement.commitment.contract.terminate"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every contract allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/agreement/commitment/contract still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "contract"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
