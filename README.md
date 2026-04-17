[![NuGet Version](https://img.shields.io/nuget/v/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution)

This repository results from the need to implement a more dynamic way to assign a document/item to the user for processing. It uses a pluggable **pipeline architecture** with scoring steps, capacity adjustment, and constraint-based allocation.

## How it works

The engine distributes items across users in a configurable pipeline of steps:

1. **Load scoring** — each user receives a base score: `(Capacity − CurrentLoad) / Capacity`. Users with more remaining capacity score higher.
2. **Working-hours capacity** *(optional)* — for part-time users, the step recalculates an effective capacity proportional to their working hours and re-scores accordingly.
3. **Priority & activity scoring** *(optional)* — adjusts the base score using the user's priority (lower value = higher priority) and recency of their last activity.
4. **Fair allocation** — distributes items one at a time in round-robin fashion, always picking the highest-scored eligible user. Constraints (e.g. max items per user) are enforced at each round.

The result contains:
- **`PrimaryUserId`** — the top-ranked user recommendation.
- **`AlternativeUserId`** — a deterministic second-choice fallback.
- **`Allocation`** — full map of every user -> number of items assigned.
- **`TotalAllocated`** — sum of all allocated items.
- **`Trace`** — audit trail showing each scoring step's output per user.

**In case you wish to use it in your project, you can install the package from <a href="https://www.nuget.org/packages/ItemDistribution" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package ItemDistribution -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)