package whyce.policy.economic.exchange.fx

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.exchange.fx.register
# Operator provisioning: register a new currency-pair definition.
allow if {
    input.policy_id == "whyce.economic.exchange.fx.register"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.fx.register"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.exchange.fx.activate
allow if {
    input.policy_id == "whyce.economic.exchange.fx.activate"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.fx.activate"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.exchange.fx.deactivate — admin or operator (terminal state).
allow if {
    input.policy_id == "whyce.economic.exchange.fx.deactivate"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.fx.deactivate"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every exchange.fx allow path requires correct route.
valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "exchange"
    input.resource.domain == "fx"
}

# Hard denies — surface missing inputs as policy violations rather than
# silently failing closed.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
