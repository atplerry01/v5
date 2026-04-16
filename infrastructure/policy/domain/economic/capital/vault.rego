package whyce.policy.economic.capital.vault

import rego.v1

default allow := false

# whyce.economic.capital.vault.create
# Owner (self-create) OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.vault.create"
    input.subject.role == "owner"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.vault.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.vault.slice_add
# Owner (of the vault) OR Admin.
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_add"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_add"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.capital.vault.slice_deposit
# Owner OR Operator. Currency match enforced by domain; policy gates by role.
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_deposit"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_deposit"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.vault.slice_allocate
# Owner only — allocation is an owner-driven decision. Operators/Admins denied.
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_allocate"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}

# whyce.economic.capital.vault.slice_release
# Owner OR Operator.
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_release"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_release"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.vault.slice_withdraw
# Owner — amount must be at or below the per-deployment withdrawal_threshold.
# Above threshold: Owner + Admin co-sign required.
# Operators and Externals are denied for withdrawals (high-impact).
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_withdraw"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    input.resource.amount <= withdrawal_threshold
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.vault.slice_withdraw"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    input.resource.amount > withdrawal_threshold
    input.subject.admin_cosign_present == true
    input.subject.trust_score >= elevated_trust_floor
    valid_resource
}

# Configurable thresholds — override via OPA data input at deployment.
# Defaults are conservative; tune per environment / currency.
withdrawal_threshold   := 10000
elevated_trust_floor   := 75

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "vault"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
