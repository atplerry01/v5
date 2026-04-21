package whyce.policy.structural.cluster.lifecycle

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.lifecycle.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.lifecycle.transition"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.lifecycle.complete"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
