package whyce.policy.structural.cluster.authority

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.authority.establish"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.establish_with_parent"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.revoke"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.reactivate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.authority.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
