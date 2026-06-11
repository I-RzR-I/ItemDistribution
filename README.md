[![NuGet Version](https://img.shields.io/nuget/v/RzR.ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.ItemDistribution/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.ItemDistribution)

<details>

  <summary>Old version</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution)

</details>

<br />

This repository results from the need to implement a more dynamic way to assign a document/item to the user for processing. It uses a pluggable **pipeline architecture** with scoring steps, capacity adjustment, and constraint-based allocation.

## How it works

The engine distributes items across users in a configurable pipeline of steps:

1. **Load scoring** — each user receives a base score: `(Capacity − CurrentLoad) / Capacity`. Users with more remaining capacity score higher.
2. **Working-hours capacity** *(optional)* — for part-time users, the step recalculates an effective capacity proportional to their working hours and re-scores accordingly.
3. **Priority & activity scoring** *(optional)* — adjusts the base score using the user's priority (lower value = higher priority) and recency of their last activity.
4. **Fair allocation** — distributes items one at a time, each round picking the eligible user with the highest `score × remaining-headroom` (headroom = effectiveCapacity − currentLoad − alreadyAllocated this run). When two users tie on that product, the one with the lower `UserId` wins. This greedy strategy favors the least-loaded, most-available user; it does not round-robin. Constraints (e.g. max items per user) are enforced at each round.

The result contains:

- **`PrimaryUserId`** — the top-ranked user recommendation (most items received; `null` if nothing was allocated).
- **`AlternativeUserId`** — a deterministic second-choice fallback (`null` if fewer than two users received items).
- **`Allocation`** — full map of every user -> number of items assigned.
- **`TotalAllocated`** — sum of all allocated items (always ≤ `RequestedDocuments`).
- **`RequestedDocuments`** — the document count passed to the engine for this run.
- **`UnallocatedDocuments`** — `Max(0, RequestedDocuments − TotalAllocated)`. Greater than 0 when total user capacity was exhausted before all requested items were placed.
- **`IsFullyAllocated`** — `true` when every requested document was placed (`UnallocatedDocuments == 0`).
- **`Trace`** — audit trail showing each scoring step's output per user.

**In case you wish to use it in your project, you can install the package from <a href="https://www.nuget.org/packages/RzR.ItemDistribution" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package RzR.ItemDistribution -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
2. [CHANGELOG](docs/CHANGELOG.md)
3. [BRANCH-GUIDE](docs/branch-guide.md)