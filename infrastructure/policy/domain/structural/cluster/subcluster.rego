package whyce.policy.structural.cluster.subcluster

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.cluster.subcluster.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.define_with_parent"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.reactivate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.archive"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.cluster.subcluster.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
