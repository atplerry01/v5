package whyce.policy.content.media.intake.ingest

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.intake.ingest.request"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.intake.ingest.accept"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.intake.ingest.start_processing"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.intake.ingest.complete"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.intake.ingest.fail"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.intake.ingest.cancel"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
