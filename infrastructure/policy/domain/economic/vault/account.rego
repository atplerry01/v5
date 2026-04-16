package whyce.policy.economic.vault.account

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.vault.account.create
# Owner self-onboard (KYC entitlement) OR Operator (system-provisioning).
allow if {
    input.policy_id == "whyce.economic.vault.account.create"
    input.subject.role == "owner"
    input.subject.kyc_passed == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.vault.account.create"
    input.subject.role == "operator"
    valid_resource
}
# Dev/test bypass — see apply_revenue admin clause for rationale.
allow if {
    input.policy_id == "whyce.economic.vault.account.create"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.vault.account.fund
# Operator (system) OR External (with attested rail signature).
# Owner-initiated funding is denied — funding flows through Operator/External.
allow if {
    input.policy_id == "whyce.economic.vault.account.fund"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.vault.account.fund"
    input.subject.role == "external"
    input.subject.attested_external_rail == true
    valid_resource
}

# whyce.economic.vault.account.invest
# Owner of the vault (moving their own Slice1 → Slice2) OR Operator.
allow if {
    input.policy_id == "whyce.economic.vault.account.invest"
    input.subject.role == "owner"
    input.subject.is_owner_of_resource == true
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.vault.account.invest"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.vault.account.apply_revenue
# Operator only — SPV profit booking is a system-driven action, never
# user-initiated. Triggered from the RevenueProcessing workflow.
allow if {
    input.policy_id == "whyce.economic.vault.account.apply_revenue"
    input.subject.role == "operator"
    valid_resource
}
# Dev/test bypass — admin role allows the action so end-to-end workflow
# validation can run without standing up an operator-roled JWT issuer.
# Mirrors the revenue domain stub pattern; remove once the operator-issued
# service-identity is wired into the host.
allow if {
    input.policy_id == "whyce.economic.vault.account.apply_revenue"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.vault.account.debit
# Operator — payout debits are system-driven from the distribution/payout
# workflows. High-impact: above threshold requires Admin co-sign.
allow if {
    input.policy_id == "whyce.economic.vault.account.debit"
    input.subject.role == "operator"
    input.resource.amount <= payout_threshold
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.vault.account.debit"
    input.subject.role == "operator"
    input.resource.amount > payout_threshold
    input.subject.admin_cosign_present == true
    input.subject.trust_score >= elevated_trust_floor
    valid_resource
}

# whyce.economic.vault.account.credit
# Operator only — credits reverse a prior debit (payout reversal / cancellation).
allow if {
    input.policy_id == "whyce.economic.vault.account.credit"
    input.subject.role == "operator"
    valid_resource
}

# Configurable thresholds — override via OPA data input at deployment.
# Defaults are conservative; tune per environment / currency.
payout_threshold     := 10000
elevated_trust_floor := 75

# Resource binding — every vault.account allow path requires correct route.
valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "vault"
    input.resource.domain == "account"
}

# Hard denies — surface missing inputs as policy violations rather than
# silently failing closed (still denied by `default allow := false`, but
# named denies emit a clearer DenialReason to the audit trail).
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
