package whyce.policy.economic.capital.account

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.capital.account.open
# Owner self-onboard (KYC entitlement on actor) OR Operator (system-provisioning).
allow if {
    input.policy_id == "whyce.economic.capital.account.open"
    input.subject.role == "owner"
    input.subject.kyc_passed == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.account.open"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.account.fund
# Operator (system) OR External (with attested rail signature).
# Owner-initiated funding is denied — funding flows through Operator/External.
allow if {
    input.policy_id == "whyce.economic.capital.account.fund"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.account.fund"
    input.subject.role == "external"
    input.subject.attested_external_rail == true
    valid_resource
}

# whyce.economic.capital.account.allocate
# Owner of the account OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.account.allocate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.account.allocate"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.account.reserve
allow if {
    input.policy_id == "whyce.economic.capital.account.reserve"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.account.reserve"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.account.release_reservation
allow if {
    input.policy_id == "whyce.economic.capital.account.release_reservation"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.account.release_reservation"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.account.freeze — Admin only, reason required.
allow if {
    input.policy_id == "whyce.economic.capital.account.freeze"
    input.subject.role == "admin"
    input.resource.reason != ""
    valid_resource
}

# whyce.economic.capital.account.close — Admin only, balances must be zero
# AND no active vaults (the runtime enforces zero-balance invariants too;
# this is the policy-side gate before the engine even runs).
allow if {
    input.policy_id == "whyce.economic.capital.account.close"
    input.subject.role == "admin"
    input.resource.total_balance == 0
    input.resource.reserved_balance == 0
    input.resource.active_vault_count == 0
    valid_resource
}

# Resource binding — every capital.account allow path requires correct route.
valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "account"
}

# Hard denies — surface missing inputs as policy violations rather than
# silently failing closed (still denied by `default allow := false`, but
# named denies emit a clearer DenialReason to the audit trail).
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
