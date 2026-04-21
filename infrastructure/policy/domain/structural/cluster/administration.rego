package whyce.policy.structural.cluster.administration

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.administration.establish"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.administration.establish_with_parent"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.administration.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.administration.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.administration.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
