package whyce.policy.structural.cluster.topology

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.topology.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.topology.validate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.topology.lock"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
