package whyce.policy.structural.cluster.cluster

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.cluster.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.archive"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.bind_authority"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.release_authority"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.bind_administration"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.cluster.release_administration"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
