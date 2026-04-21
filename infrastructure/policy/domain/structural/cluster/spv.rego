package whyce.policy.structural.cluster.spv

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.spv.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.create_with_parent"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.close"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.reactivate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.spv.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
