# CHANGELOG

### **v2.0.0.6122** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 17-04-2026
-> **Breaking**: Replaced the old `DistributionSuggestionHelper` singleton with a generic, pipeline-based engine.
- New public entry point: `DistributionSuggestionHelper<TUserId>` (constructed with `EngineConfig`).
- New input model: `UserLoad<TUserId>` (immutable) replaces `UserInfoOptions<TUserId>`.
- New result model: `DistributionResult<TUserId>` with `PrimaryUserId`, `AlternativeUserId`, `Allocation`, `TotalAllocated`, and `Trace`.
- Pluggable pipeline architecture: `LoadScoringStep`, `WorkingHoursCapacityStep`, `PriorityActivityScoringStep`, `FairAllocationStep`.
- Constraint support via `IConstraint<TUserId>` (e.g. `MaxPerUserConstraint`).
- Audit trail (`DistributionTrace<TUserId>`) written by each pipeline step.
- `IDistributionSuggestionHelper<TUserId>` interface for DI and testing.
- Full async support with `CancellationToken`.


### **v1.1.0.0** 
-> Update reference package version, fixing CVE (`CVE-2024-43485`).
