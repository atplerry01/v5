package whyce.policy.structural.cluster.provider

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.provider.register"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.provider.register_with_parent"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.provider.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.provider.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.provider.reactivate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.provider.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
