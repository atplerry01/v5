# TITLE
Infrastructure Final Hardening (Pre-Validation)

# CONTEXT
- Classification: operational-system
- Context: deployment
- Domain: infrastructure

# OBJECTIVE
Complete missing domain-critical components in the restructured infrastructure: projection schemas, domain policies, Kafka topic parity, environment binding, and cross-layer domain consistency validation.

# CONSTRAINTS
- No restructuring — completion only
- All layers must use identical classification > context > domain paths
- Projection schemas must exist for every active domain
- Domain policies must exist for every active domain
- Kafka topics must cover all active domains (dual-topic + DLQ)
- Environments must bind to deployment

# EXECUTION STEPS
1. Create projection schemas: data/postgres/projections/{classification}/{context}/{domain}/001_projection.sql
2. Create domain policies: policy/domain/{classification}/{context}/{domain}.rego
3. Add missing Kafka topic definitions for identity.access.identity + operational.global.incident
4. Update create-topics.sh to cover all 3 active domains (12 topics total)
5. Bind environments to bootstrap.sh (environment parameter, config validation, export)
6. Add postgres-projections service to docker-compose.yml
7. Update OPA mounts to load both base and domain policies
8. Validate domain path consistency across kafka, postgres, redis, policy

# OUTPUT FORMAT
Change report, domain coverage check, final score, PASS/FAIL status.

# VALIDATION CRITERIA
- All 3 domains have: projection schema + Kafka topics + OPA policy + Redis namespace
- create-topics.sh creates 12 topics (4 per domain)
- bootstrap.sh accepts environment parameter and validates config
- OPA loads both base and domain policies
- docker-compose has projections postgres service
