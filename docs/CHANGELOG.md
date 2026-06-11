### **v3.0.0.7577** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 11-06-2026
* [7235013] (RzR) -> Auto commit uncommited files
* [0304e62] (RzR) -> Auto commit uncommited files
* [09b40ba] (RzR) -> Rename to RzR.ItemDistribution, narrow step contract, and harden engine

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
