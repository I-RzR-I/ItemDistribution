# CHANGELOG

### **v3.0.0.7577** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 11-06-2026
* [FIX] - (RzR) -> Working-hours effective capacity is now a hard allocation cap, not a scoring nudge — a user cannot receive more items than `Truncate(WorkingHours × MaxAllowedDocs / FullWorkDayHours)`.
* [FIX] - (RzR) -> Allocation scoring uses `score × remaining-headroom` (greedy) instead of a plain score comparison, so the most-available user is consistently preferred over round-robin rotation.
* [FIX] - (RzR) -> `PriorityActivityScoringStep` priority denominator is always ≥ 1 regardless of sign, eliminating the divide-by-zero that occurred when all users shared the same priority value.
* [FIX] - (RzR) -> `PrimaryUserId` / `AlternativeUserId` tie-break in `DistributionResult` mirrors the allocator's own ascending `UserId` order, making both properties deterministic across runs.
* [FIX] - (RzR) -> `Allocation` and `Trace` in `DistributionResult` are defensive copies — mutating the returned dictionary or collection does not affect subsequent engine runs.
* [FIX] - (RzR) -> `CancellationToken` is checked between pipeline steps; a pre-cancelled token produces a failed `IResult` rather than throwing `OperationCanceledException` to the caller.
* [FIX] - (RzR) -> `UserLoad<TUserId>` constructor throws `ArgumentOutOfRangeException` for negative `currentLoad`, `capacity`, or `workingHours`; the helper surfaces such exceptions as a failed `IResult`.
* [DEV] - (RzR) -> `DistributionResult<TUserId>` exposes `RequestedDocuments`, `UnallocatedDocuments` (`Max(0, Requested − TotalAllocated)`), and `IsFullyAllocated` to signal partial-allocation runs.
* [DEV] - (RzR) -> `EngineConfig<TUserId>` with `AdditionalSteps` (`IReadOnlyList<IDistributionStep<TUserId>>`) and `AdditionalConstraints` (`IReadOnlyList<IConstraint<TUserId>>`), plus a matching `DistributionSuggestionHelper<TUserId>(EngineConfig<TUserId>)` constructor overload for consumer-supplied pipeline extensibility.
* [DEV] - (RzR) -> `IDistributionStep<TUserId>.Execute` now receives `IDistributionContext<TUserId>` (a narrowed, read-only-plus-explicit-mutation contract) instead of the raw mutable `DistributionContext<TUserId>`. Custom steps must use `SetScore`/`SetEffectiveCapacity`/`IncrementAllocation`/`AddTrace` to mutate state; direct dictionary access (`context.Scores[id] = x`, `context.Allocation[id]++`, `context.Trace.Add(...)`) is no longer possible. `DistributionContext<TUserId>` no longer exposes raw mutable `IDictionary`/`IList` properties — `Scores`, `EffectiveCapacity`, and `Allocation` are now `IReadOnlyDictionary`, and `Trace` is no longer public. The `Abstractions.Pipeline` layer no longer depends on `Engine.Pipeline` (the layering smell is removed).

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
