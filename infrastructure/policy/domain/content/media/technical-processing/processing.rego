package whyce.policy.content.media.technical_processing.processing

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.technical_processing.processing.request"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.technical_processing.processing.start"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.technical_processing.processing.complete"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.technical_processing.processing.fail"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.technical_processing.processing.cancel"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
