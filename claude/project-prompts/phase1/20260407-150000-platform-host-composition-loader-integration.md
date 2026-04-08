# WHYCESPACE — COMPOSITION MODULE LOADER INTEGRATION (SAFE PATCH)

CLASSIFICATION: platform / host / composition-loader-integration
MODE: SAFE PATCH (NO BEHAVIOR CHANGE)
PRIORITY: HIGH
STORED: 2026-04-07

## CONTEXT
Introduce a deterministic composition module loader alongside BootstrapModuleCatalog
without behavior change. Existing per-category Add*Composition extensions remain
the registration source; modules merely orchestrate them under a strict order.

## OBJECTIVE
Wrap existing composition extensions with ICompositionModule, register them in a
deterministic CompositionRegistry, expose a CompositionModuleLoader, and route
Program.cs through it while keeping BootstrapModuleCatalog intact.

## CONSTRAINTS
- No behavior change
- BootstrapModuleCatalog remains domain source of truth
- No reflection auto-discovery
- Strict ordering via Order property
- Anti-drift ($5): wrap only modules that already exist; do not invent
  Systems / Security composition modules that have no underlying extension.

## EXECUTION STEPS
1. Create ICompositionModule contract under composition/abstractions/.
2. Wrap each existing Add*Composition extension as an ICompositionModule.
3. Create CompositionRegistry with deterministic Order.
4. Create CompositionModuleLoader extension.
5. Update Program.cs to call LoadModules; keep BootstrapModuleCatalog loop.
6. Create composition-loader.guard.md.
7. Update program-composition.guard.md ALLOWED list to permit loader call.
8. Build + verify.

## OUTPUT FORMAT
Created files list, registry contents, execution order, behavior comparison.

## VALIDATION CRITERIA
- dotnet build passes
- Program.cs ≤100 lines, composition-only
- BootstrapModuleCatalog loop intact
- Module Order deterministic