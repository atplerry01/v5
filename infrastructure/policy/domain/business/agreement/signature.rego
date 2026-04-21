package whyce.policy.business.agreement.signature

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.business.agreement.party-governance.signature.create
# Authoring party (owner) OR Operator (system-provisioning) may create.
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.signature.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.signature.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.party-governance.signature.sign
# Only the owner of the signature (signatory) or an operator may sign.
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.signature.sign"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.signature.sign"
    input.subject.role == "operator"
    valid_resource
}

# whyce.business.agreement.party-governance.signature.revoke
# Admin only — revocation is an authoritative action.
allow if {
    input.policy_id == "whyce.business.agreement.party-governance.signature.revoke"
    input.subject.role == "admin"
    valid_resource
}

# Resource binding — every signature allow path requires correct route.
# DomainRoute is (classification, context, domain); the four-level physical
# path business/agreement/party-governance/signature still projects onto this tuple.
valid_resource if {
    input.resource.classification == "business"
    input.resource.context == "agreement"
    input.resource.domain == "signature"
}

# Hard denies — surface missing inputs as named policy violations.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
