# USING

## Quick start

Create a `DistributionSuggestionHelper<TUserId>` with an `EngineConfig`, then call `Generate(...)` or `GenerateAsync(...)`.

```csharp
using ItemDistribution.Models.Config;
using ItemDistribution.Models.Domain;
using ItemDistribution.Public;

// 1. Configure the engine
var sut = new DistributionSuggestionHelper<Guid>(new EngineConfig
{
    UsePriorityAndActivity = true,   // enable priority & activity scoring
    FullWorkDayHours = 8,      // hours in a full working day
    MaxAllowedInProcessDocuments = 15,    // max docs a full-day user can process
    MaxPerUser = null    // optional cap per user per run
});

// 2. Build the user list
var users = new List<UserLoad<Guid>>
{
    new UserLoad<Guid>(
        userId: Guid.NewGuid(),
        currentLoad: 1,
        capacity: 15,
        priority: 5,
        lastActivityDate: DateTime.Now.AddDays(-1),
        workingHours: 8),
    new UserLoad<Guid>(
        userId: Guid.NewGuid(),
        currentLoad: 8,
        capacity: 15,
        priority: 5,
        lastActivityDate: DateTime.Now.AddDays(-1.2),
        workingHours: 8)
};

// 3. Distribute 1 item
var result = sut.Generate(users, documents: 1);

if (result.IsSuccess)
{
    var r = result.Response;
    Console.WriteLine($"Primary:     {r.PrimaryUserId}");
    Console.WriteLine($"Alternative: {r.AlternativeUserId}");
    Console.WriteLine($"Total:       {r.TotalAllocated}");
}
```

## Configuration — `EngineConfig`

| Property | Type | Default | Description |
|---|---|---|---|
| `MaxPerUser` | `int?` | `null` | Maximum items allocated to a single user in one run. Must be > 0 when set. |
| `UsePriorityAndActivity` | `bool` | `false` | When `true`, enables the priority & activity scoring step after load scoring. |
| `FullWorkDayHours` | `decimal` | `8` | Hours in a standard full working day. Required with `MaxAllowedInProcessDocuments`. |
| `MaxAllowedInProcessDocuments` | `int` | `0` | Max documents a full-day user can process. When > 0, enables working-hours capacity adjustment. |

## User input — `UserLoad<TUserId>`

Each user is described by an immutable `UserLoad<TUserId>`:

| Property | Type | Description |
|---|---|---|
| `UserId` | `TUserId` | Unique identifier of the user. |
| `CurrentLoad` | `decimal` | Number of items currently assigned to / in process by this user. |
| `Capacity` | `decimal` | Maximum number of items this user can handle. |
| `Priority` | `short` | Selection priority — lower value = higher priority (closer to 0 wins). Default `0`. |
| `LastActivityDate` | `DateTime` | User's last recorded activity date/time. |
| `WorkingHours` | `decimal` | Hours worked per day (e.g. 4 for part-time). Default `0` = capacity used as-is. |

## Result — `DistributionResult<TUserId>`

The `Generate` / `GenerateAsync` methods return `IResult<DistributionResult<TUserId>>`:

| Property | Type | Description |
|---|---|---|
| `PrimaryUserId` | `TUserId?` | Top-ranked user — the primary recommendation. `null` when nothing could be allocated. |
| `AlternativeUserId` | `TUserId?` | Second-ranked user — a deterministic fallback. `null` when fewer than 2 users received items. |
| `Allocation` | `IDictionary<TUserId, int>` | Full map of every user → how many items the engine assigned. |
| `TotalAllocated` | `int` | Sum of all values in `Allocation`. |
| `Trace` | `IReadOnlyList<DistributionTrace<TUserId>>` | Audit trail written by each pipeline step (useful for diagnostics). |

## Pipeline steps

The engine executes the following steps in order:

1. **`LoadScoringStep`** — scores each user as `(Capacity − CurrentLoad) / Capacity`.
2. **`WorkingHoursCapacityStep`** *(when `MaxAllowedInProcessDocuments > 0`)* — recalculates effective capacity for part-time users: `effectiveCapacity = Truncate((WorkingHours × MaxAllowedDocs) / FullWorkDayHours)`.
3. **`PriorityActivityScoringStep`** *(when `UsePriorityAndActivity = true`)* — multiplicatively adjusts scores using priority and activity recency.
4. **`FairAllocationStep`** — distributes items one-by-one in round-robin, picking the highest-scored eligible user each round. Constraints (e.g. `MaxPerUserConstraint`) are enforced.

## Async usage

```csharp
var result = await sut.GenerateAsync(users, documents: 5, cancellationToken);
```

The async overload runs the pipeline on a thread-pool thread and supports `CancellationToken`.

---

## Migrating from v1.x

This section walks through every change needed to move from the old `DistributionSuggestionHelper` singleton to the new generic, pipeline-based API.

### Step 1 — Replace `using` directives

```diff
- using ItemDistribution.Helpers;
- using ItemDistribution.Models.Dto;
+ using ItemDistribution.Models.Config;
+ using ItemDistribution.Models.Domain;
+ using ItemDistribution.Public;
```

### Step 2 — Replace the singleton with a configured instance

```diff
- var helper = DistributionSuggestionHelper.Instance;
+ var helper = new DistributionSuggestionHelper<Guid>(new EngineConfig
+ {
+     UsePriorityAndActivity = true,
+     FullWorkDayHours = 8, // was DistributionParams.FullWorkDayHours
+     MaxAllowedInProcessDocuments = 15 // was DistributionParams.MaxAllowedInProcessDocuments
+ });
```

> `FullWorkDayHours` and `MaxAllowedInProcessDocuments` move from per-call parameters to engine-level configuration.
> `AvoidDuplicateResult` is removed — the alternative user is now always deterministic (second-ranked).

### Step 3 — Replace `UserInfoOptions` with `UserLoad`

```diff
- new UserInfoOptions<Guid>
- {
-     UserId = id,
-     UserName = "Alice",
-     InProcessDocuments = 3,
-     WorkingHours = 8,
-     UserPriority = 5,
-     LastActivityDate = DateTime.Now.AddDays(-1)
- }
+ new UserLoad<Guid>(
+     userId: id,
+     currentLoad: 3, // was InProcessDocuments
+     capacity: 15, // was derived from MaxAllowedInProcessDocuments
+     priority: 5, // was UserPriority
+     lastActivityDate: DateTime.Now.AddDays(-1),
+     workingHours: 8)
```

| v1 property | v2 parameter | Notes |
|---|---|---|
| `UserId` | `userId` | Same. |
| `UserName` | *(removed)* | Not needed by the engine. Track externally if needed for display. |
| `InProcessDocuments` | `currentLoad` | Renamed. Same semantics. |
| *(derived at runtime)* | `capacity` | Now explicit — set it to `MaxAllowedInProcessDocuments` or the user's individual cap. |
| `UserPriority` | `priority` | Renamed. Same semantics (lower = higher priority). |
| `LastActivityDate` | `lastActivityDate` | Same. |
| `WorkingHours` | `workingHours` | Same. |

### Step 4 — Replace the method call

```diff
- var result = helper.GenerateNewDistributionSuggestion(
-     new DistributionParams<Guid>
-     {
-         FullWorkDayHours = 8,
-         MaxAllowedInProcessDocuments = 15,
-         EligibleRepartitionUsers = users,
-         AvoidDuplicateResult = true
-     });
+ var result = helper.Generate(users, documents: 1);
```

For async:
```diff
- var result = await helper.GenerateNewDistributionSuggestionAsync(params);
+ var result = await helper.GenerateAsync(users, documents: 1, cancellationToken);
```

### Step 5 — Update result access

```diff
- if (result.IsSuccess)
- {
-     var suggested = result.Response.SuggestionUser.UserName;
-     var altUser = result.Response.AlternativeUser?.UserName;
-     var inProcess = result.Response.SuggestionUser.InProcessItems;
- }
+ if (result.IsSuccess)
+ {
+     var primaryId = result.Response.PrimaryUserId; // TUserId?
+     var altId = result.Response.AlternativeUserId; // TUserId?
+     var total = result.Response.TotalAllocated; // int
+     var allocation = result.Response.Allocation; // IDictionary<TUserId, int>
+     var trace = result.Response.Trace; // IReadOnlyList<DistributionTrace<TUserId>>
+ }
```

| v1 property | v2 property | Notes |
|---|---|---|
| `Response.SuggestionUser.SuggestionUserId` | `Response.PrimaryUserId` | Top-ranked user. |
| `Response.AlternativeUser.SuggestionUserId` | `Response.AlternativeUserId` | Second-ranked (deterministic, not random). |
| `Response.SuggestionUser.UserName` | *(removed)* | Track names externally via a `Dictionary<TUserId, string>`. |
| `Response.SuggestionUser.InProcessItems` | *(removed)* | Already known from `UserLoad.CurrentLoad`. |
| *(not available)* | `Response.Allocation` | Full distribution map — new in v2. |
| *(not available)* | `Response.TotalAllocated` | Sum of allocated items — new in v2. |
| *(not available)* | `Response.Trace` | Audit trail — new in v2. |

### Step 6 — Remove `.Dispose()` call

```diff
- helper.Dispose();
  // DistributionSuggestionHelper<TUserId> is not IDisposable — no cleanup needed.
```

### Step 7 — DI registration (optional)

The new API exposes `IDistributionSuggestionHelper<TUserId>` for dependency injection:

```csharp
services.AddSingleton<IDistributionSuggestionHelper<Guid>>(
    new DistributionSuggestionHelper<Guid>(new EngineConfig
    {
        UsePriorityAndActivity = true,
        FullWorkDayHours = 8,
        MaxAllowedInProcessDocuments = 15
    }));
```
