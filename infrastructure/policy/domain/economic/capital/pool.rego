package whyce.policy.economic.capital.pool

import rego.v1

default allow := false

# Every pool action is system-only — pools are aggregation infrastructure,
# not user-facing aggregates. Owners and externals are denied by default.

# whyce.economic.capital.pool.create — one pool per currency.
allow if {
    input.policy_id == "whyce.economic.capital.pool.create"
    input.subject.role == "operator"
    valid_resource
}

# whyce.economic.capital.pool.aggregate — roll account capital into pool.
allow if {
    input.policy_id == "whyce.economic.capital.pool.aggregate"
    input.subject.role == "operator"
    input.resource.amount > 0
    valid_resource
}

# whyce.economic.capital.pool.reduce — withdraw from pool back to account.
# Domain-side invariant ensures TotalCapital >= amount; policy gates by role.
allow if {
    input.policy_id == "whyce.economic.capital.pool.reduce"
    input.subject.role == "operator"
    input.resource.amount > 0
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "pool"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
