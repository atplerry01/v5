package whyce.policy["whyce-policy-default"]

import rego.v1

# Sentinel package for the legacy `whyce-policy-default` policy_id used when a
# command has no canonical binding registered via ICommandPolicyIdRegistry.
# OpaPolicyEvaluator preserves the hyphenated id verbatim and queries
# /v1/data/whyce/policy/whyce-policy-default. Without this rule the URL
# resolves to {} and the runtime treats the absent `allow` field as deny.
#
# Default-deny preserved; admin role bypasses for development/testing only.
# Production must register canonical policy bindings instead of relying on
# this sentinel.

default allow := false

allow if { input.subject.role == "admin" }
