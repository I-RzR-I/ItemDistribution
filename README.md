[![NuGet Version](https://img.shields.io/nuget/v/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/ItemDistribution.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ItemDistribution)

This repository results from the need to implement a more dynamic way to assign and document/item to the user for processing. At the base of implementation is a selection that must find the user that is move available and has a few items in processing (coefficient of load).

This is a short description of the base algorithm used for user selection.

The user selection is implemented in a few steps:
- Find the user with the lowest coefficient;
- In case when was found more than one user with the same coefficient, then find a new user by his `priority` (`Priority` may be the user role, user qualification, or user work experience, etc. The priority is higher as close to 0 as possible is the value.) and return new selection;
- In case when was found more than one user with the same `priority`, then find the first user with the most recent activity date.
- In all-over cases, like an exception, the algorithm will get a random user.

As a result of algorithm execution, you will get 2 user suggestions:
-  a `base` user distribution suggestion 
- an `alternative` user suggestion (which is a random user selection).

**In case you wish to use it in your project, u can install the package from <a href="https://www.nuget.org/packages/ItemDistribution" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package ItemDistribution -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)