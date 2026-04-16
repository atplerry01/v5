package whyce.policy.economic.revenue.contract

import rego.v1

# Stub policy — author allow rules per command as the revenue surface matures.
# Allows any action invoked by the revenue-admin role.
# OpaPolicyEvaluator iterates roles per evaluation and sends `input.subject.role`
# as a singular value, so the user-intent `roles[_] == "revenue-admin"` maps to
# `input.subject.role == "revenue-admin"` below.

default allow := false

allow if {
    input.subject.role == "revenue-admin"
}

deny if { not input.subject.role }
