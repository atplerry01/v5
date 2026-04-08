# TITLE
Enterprise Infrastructure Restructure (WBSM v3.5)

# CONTEXT
- Classification: operational-system
- Context: deployment
- Domain: infrastructure

# OBJECTIVE
Transform /infrastructure from tool-based flat folders (kafka/, postgres/, redis/, opa/) to enterprise-grade domain-aligned structure: data/, event-fabric/, policy/, observability/, deployment/, environments/, security/, docs/.

# CONSTRAINTS
- All configs must follow classification > context > domain nesting
- Kafka topics must follow domain structure
- Postgres split into event-store, projections, system
- OPA split into base and domain-specific
- Docker-compose moves to deployment/ with updated volume paths
- Legacy flat structure must be fully removed

# EXECUTION STEPS
1. Create root enterprise directories
2. Move postgres, redis, minio into data/ (postgres split into event-store/projections/system)
3. Move kafka into event-fabric/ with domain-aligned topics
4. Move OPA into policy/ (base + domain)
5. Restructure observability (prometheus + grafana separated)
6. Create deployment/ with docker-compose and bootstrap/migration/teardown scripts
7. Create environments/ (local, dev, staging, production) with environment.json
8. Create security/ and docs/ placeholders
9. Update all docker-compose.yml volume mounts to new relative paths
10. Remove legacy flat structure

# OUTPUT FORMAT
Change report, new structure tree, enterprise alignment check, domain alignment check, final score.

# VALIDATION CRITERIA
- No flat tool-based folders remain at infrastructure root
- All volume mounts in docker-compose.yml point to correct new paths
- Topic naming follows whyce.{classification}.{context}.{domain}.{type}
- Environment configs follow domain model classification
- Bootstrap/migration scripts reference correct paths
